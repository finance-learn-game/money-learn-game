using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using R3;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace SmbcApp.LearnGame.UIWidgets.Chart
{
    public sealed class ChartGraphic : Graphic
    {
        [SerializeField] [BoxGroup("Svg")] private float svgPixelsPerUnit = 100f;
        [SerializeField] [BoxGroup("Line")] private float lineThickness;
        [SerializeField] [BoxGroup("Line")] private float stepDistance = 10f;
        [SerializeField] [BoxGroup("Line")] private ChartData[] chartDataArr;
        [SerializeField] [BoxGroup("Line")] private float2 dataRange;
        [SerializeField] [BoxGroup("Circle")] private float circleRadius;
        [SerializeField] [BoxGroup("Circle")] private float circleThickness;
        [SerializeField] [BoxGroup("Circle")] private Color circleFillColor;
        [SerializeField] [BoxGroup("Grid")] private Color gridColor;
        [SerializeField] [BoxGroup("Grid")] private float gridThickness;
        [SerializeField] [BoxGroup("Grid")] private float gridStepDistance = 10f;

        private readonly Subject<ChartData[]> _onDataChanged = new();
        private List<VectorUtils.Geometry> _geomList;

        public float2 YDataRange
        {
            get => dataRange;
            set => SetData(chartDataArr, value);
        }

        public float GridStep => gridStepDistance;
        public Observable<ChartData[]> OnDataChanged => _onDataChanged;

        private float2 XRange => new(RectTransform.rect.xMin, RectTransform.rect.xMax);
        private float2 YRange => new(RectTransform.rect.yMin, RectTransform.rect.yMax);

        public IEnumerable<float> YGridPositions => Enumerable
            .Range(0, (int)((dataRange.y - dataRange.x) / gridStepDistance) + 1)
            .Select(i => i * gridStepDistance + dataRange.x)
            .Select(y => math.remap(
                dataRange.x, dataRange.y,
                YRange.x, YRange.y,
                y
            ));

        private RectTransform RectTransform => transform as RectTransform;

        [Button]
        private void Configure()
        {
            var shapes = new List<Shape> { GridShape() };
            foreach (var chartData in chartDataArr.Where(data => data.show))
            {
                var calcData = CalcData(chartData.data);
                shapes.Add(ChartShape(calcData, chartData.color));
                shapes.AddRange(PointCircleShape(calcData, chartData.color));
            }

            var scene = new Scene { Root = new SceneNode { Shapes = shapes } };
            UniTask.Void(async () =>
            {
                _geomList = await UniTask.RunOnThreadPool(() =>
                    VectorUtils.TessellateScene(scene, GetTessellationOptions())
                );
                SetAllDirty();
            });
        }

        /// <summary>
        ///     表示用のデータを設定する
        /// </summary>
        /// <param name="dataArr">データ</param>
        /// <param name="range">データの幅、指定なしの場合は自動設定</param>
        public void SetData(ChartData[] dataArr, float2 range = default)
        {
            chartDataArr = dataArr;
            dataRange = range.Equals(default) ? AutoSetDataRange() : range;
            Configure();
            _onDataChanged.OnNext(dataArr);

            return;

            float2 AutoSetDataRange()
            {
                var min = math.floor(dataArr.Min(d => d.data.Min()) / GridStep) * GridStep;
                var max = math.ceil(dataArr.Max(d => d.data.Max()) / GridStep) * GridStep;
                return new float2(min, max);
            }
        }

        public void SetChartShow(int dataIndex, bool isShow)
        {
            if (dataIndex < 0 || dataIndex >= chartDataArr.Length) return;

            chartDataArr[dataIndex].show = isShow;
            Configure();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            if (_geomList == null) return;

            vh.Clear();

            var verts = ListPool<UIVertex>.Get();
            var indices = ListPool<int>.Get();
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
            ListPool<UIVertex>.Release(verts);
            ListPool<int>.Release(indices);
        }

        private float2[] CalcData(float[] data)
        {
            var xStep = (XRange.y - XRange.x) / (data.Length - 1);

            var resData = new float2[data.Length];
            for (var i = 0; i < data.Length; i++)
            {
                var x = XRange.x + i * xStep;
                var y = math.remap(dataRange.x, dataRange.y, YRange.x, YRange.y, data[i]);
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

        private Shape ChartShape(float2[] data, Color chartColor)
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

            return new Shape
            {
                Contours = new[]
                {
                    new BezierContour
                    {
                        Segments = segments.ToArray(),
                        Closed = false
                    }
                },
                PathProps = GetPathProperties(lineThickness, chartColor)
            };
        }

        private Shape[] PointCircleShape(float2[] data, Color circleColor)
        {
            var shapes = new Shape[data.Length];
            for (var i = 0; i < data.Length; i++)
            {
                var point = data[i];
                shapes[i] = new Shape
                {
                    PathProps = GetPathProperties(circleThickness, circleColor, PathCorner.Round,
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

        /// <summary>
        ///     チャートのデータを表す構造体
        /// </summary>
        [Serializable]
        public struct ChartData
        {
            public float[] data;
            public Color color;
            public string label;
            public bool show;

            public ChartData(float[] data, int colorIndex, string label)
            {
                this.data = data;
                color = HsvColor(colorIndex);
                this.label = label;
                show = true;
            }

            private static Color HsvColor(int colorIndex)
            {
                const int palette = 10;
                return Color.HSVToRGB(colorIndex % palette / (float)palette, 1, 1);
            }

            [Button]
            private void SetHsvColor(int colorIndex)
            {
                color = HsvColor(colorIndex);
            }
        }
    }
}