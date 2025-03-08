using System.Collections.Generic;
using System.Linq;
using R3;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.UI;

namespace SmbcApp.LearnGame.UIWidgets.Chart
{
    public sealed class ChartGraphic : Graphic
    {
        [SerializeField] [BoxGroup("Svg")] private float svgPixelsPerUnit = 100f;
        [SerializeField] [BoxGroup("Line")] private float lineThickness;
        [SerializeField] [BoxGroup("Line")] private float stepDistance = 10f;
        [SerializeField] [BoxGroup("Line")] private float[] yData;
        [SerializeField] [BoxGroup("Line")] private float2 yDataRange;
        [SerializeField] [BoxGroup("Circle")] private float circleRadius;
        [SerializeField] [BoxGroup("Circle")] private float circleThickness;
        [SerializeField] [BoxGroup("Circle")] private Color circleStrokeColor;
        [SerializeField] [BoxGroup("Circle")] private Color circleFillColor;
        [SerializeField] [BoxGroup("Grid")] private Color gridColor;
        [SerializeField] [BoxGroup("Grid")] private float gridThickness;
        [SerializeField] [BoxGroup("Grid")] private float gridStepDistance = 10f;

        private readonly Subject<float[]> _onDataChanged = new();
        private List<VectorUtils.Geometry> _geomList;

        public float2 YDataRange
        {
            get => yDataRange;
            set => SetData(yData, value);
        }

        public float GridStep => gridStepDistance;
        public Observable<float[]> OnDataChanged => _onDataChanged;

        private float2 XRange => new(RectTransform.rect.xMin, RectTransform.rect.xMax);
        private float2 YRange => new(RectTransform.rect.yMin, RectTransform.rect.yMax);

        public IEnumerable<float> YGridPositions => Enumerable
            .Range(0, (int)((yDataRange.y - yDataRange.x) / gridStepDistance) + 1)
            .Select(i => i * gridStepDistance + yDataRange.x)
            .Select(y => math.remap(
                yDataRange.x, yDataRange.y,
                YRange.x, YRange.y,
                y
            ));

        private RectTransform RectTransform => transform as RectTransform;

        [Button]
        private void Configure()
        {
            var shapes = new List<Shape>
            {
                GridShape(),
                new()
                {
                    Contours = new[]
                    {
                        ChartContour(CalcData(yData))
                    },
                    PathProps = GetPathProperties(lineThickness, color)
                }
            };
            shapes.AddRange(PointCircleShape(CalcData(yData)));

            var scene = new Scene { Root = new SceneNode { Shapes = shapes } };
            _geomList = VectorUtils.TessellateScene(scene, GetTessellationOptions());

            SetAllDirty();
        }

        /// <summary>
        ///     表示用のデータを設定する
        /// </summary>
        /// <param name="data">データ</param>
        /// <param name="dataRange">データの幅、指定なしの場合は自動設定</param>
        public void SetData(float[] data, float2 dataRange = default)
        {
            yData = data;
            yDataRange = dataRange.Equals(default) ? AutoSetDataRange() : dataRange;
            Configure();
            _onDataChanged.OnNext(data);

            return;

            float2 AutoSetDataRange()
            {
                var min = math.floor(data.Min() / GridStep) * GridStep;
                var max = math.ceil(data.Max() / GridStep) * GridStep;
                return new float2(min, max);
            }
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            if (_geomList == null) return;

            vh.Clear();

            var verts = new List<UIVertex>();
            var indices = new List<int>();
            foreach (var geometry in _geomList)
            {
                var indexOffset = verts.Count;

                verts.AddRange(geometry.Vertices
                    .Select(vert => new UIVertex
                    {
                        position = new float3(geometry.WorldTransform * vert / svgPixelsPerUnit, 0),
                        color = geometry.Color
                    })
                );
                indices.AddRange(geometry.Indices.Select(idx => idx + indexOffset));
            }

            vh.AddUIVertexStream(verts, indices);
        }

        private float2[] CalcData(float[] data)
        {
            var xStep = (XRange.y - XRange.x) / (data.Length - 1);

            var resData = new float2[data.Length];
            for (var i = 0; i < data.Length; i++)
            {
                var x = XRange.x + i * xStep;
                var y = math.remap(yDataRange.x, yDataRange.y, YRange.x, YRange.y, data[i]);
                resData[i] = new float2(x, y);
            }

            return resData;
        }

        private Shape GridShape()
        {
            var contours = from y in YGridPositions
                let p0 = new float2(XRange.x, y)
                let p1 = new float2(XRange.y, y)
                select new BezierContour
                {
                    Segments = VectorUtils.MakePathLine(p0, p1).ToArray(),
                    Closed = false
                };
            return new Shape
            {
                Contours = contours.ToArray(),
                PathProps = GetPathProperties(gridThickness, gridColor, PathCorner.Tipped, PathEnding.Square)
            };
        }

        private static BezierContour ChartContour(float2[] data)
        {
            var segments = new BezierPathSegment[data.Length];

            for (var i = 0; i < data.Length - 1; i++)
            {
                var curr = data[i];
                var next = data[i + 1];

                var lineSegments = VectorUtils.MakePathLine(curr, next);
                segments[i] = lineSegments[0];
                if (i == data.Length - 2)
                    segments[i + 1] = lineSegments[1];
            }

            return new BezierContour
            {
                Segments = segments.ToArray(),
                Closed = false
            };
        }

        private Shape[] PointCircleShape(float2[] data)
        {
            var shapes = new Shape[data.Length];
            for (var i = 0; i < data.Length; i++)
            {
                var point = data[i];
                shapes[i] = new Shape
                {
                    PathProps = GetPathProperties(circleThickness, circleStrokeColor, PathCorner.Round,
                        PathEnding.Square),
                    Fill = new SolidFill { Color = circleFillColor }
                };
                VectorUtils.MakeCircleShape(shapes[i], point, circleRadius);
            }

            return shapes;
        }

        private static PathProperties GetPathProperties(float thickness, Color pathColor,
            PathCorner corner = PathCorner.Round, PathEnding ending = PathEnding.Round)
        {
            return new PathProperties
            {
                Stroke = new Stroke
                {
                    Color = pathColor,
                    HalfThickness = thickness * 0.5f
                },
                Corners = corner,
                Head = ending,
                Tail = ending
            };
        }

        private VectorUtils.TessellationOptions GetTessellationOptions()
        {
            return new VectorUtils.TessellationOptions
            {
                StepDistance = stepDistance,
                MaxCordDeviation = 0.05f,
                MaxTanAngleDeviation = 0.05f,
                SamplingStepSize = 0.01f
            };
        }
    }
}