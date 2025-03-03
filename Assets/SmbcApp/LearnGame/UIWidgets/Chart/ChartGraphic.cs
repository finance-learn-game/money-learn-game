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
    internal sealed class ChartGraphic : Graphic
    {
        [SerializeField] [BoxGroup("Line")] private float lineThickness;
        [SerializeField] [BoxGroup("Line")] private float stepDistance = 10f;
        [SerializeField] [BoxGroup("Line")] private float[] yData;
        [SerializeField] [BoxGroup("Line")] private float2 yDataRange;
        [SerializeField] [BoxGroup("Grid")] private Color gridColor;
        [SerializeField] [BoxGroup("Grid")] private float gridThickness;
        [SerializeField] [BoxGroup("Grid")] private float gridStepDistance = 10f;
        private readonly Subject<float[]> _onDataChanged = new();

        private List<int> _indices;
        private List<UIVertex> _vertices;

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
            _vertices ??= new List<UIVertex>();
            _indices ??= new List<int>();
            _vertices.Clear();
            _indices.Clear();

            DrawGrid();
            DrawLine(CalcData(yData));
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
            vh.Clear();

            if (_vertices == null || _indices == null) return;
            if (_vertices.Count == 0 || _indices.Count == 0) return;

            vh.AddUIVertexStream(_vertices, _indices);
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

        private void DrawGrid()
        {
            foreach (var y in YGridPositions)
            {
                var p0 = new float2(XRange.x, y);
                var p1 = new float2(XRange.y, y);
                VectorUtils.TessellatePath(
                    new BezierContour
                    {
                        Segments = VectorUtils.MakePathLine(p0, p1).ToArray(),
                        Closed = false
                    },
                    GetPathProperties(gridThickness, gridColor, PathCorner.Tipped, PathEnding.Square),
                    GetTessellationOptions(),
                    out var vertices,
                    out var indices
                );
                AddUiVertexes(vertices, indices, gridColor);
            }
        }

        private void DrawLine(float2[] data)
        {
            var segments = new List<BezierPathSegment>();

            for (var i = 0; i < data.Length - 1; i++)
            {
                var curr = data[i];
                var next = data[i + 1];

                var lineSegments = VectorUtils.MakePathLine(curr, next);
                segments.Add(lineSegments[0]);
                if (i == data.Length - 2) segments.Add(lineSegments[1]);
            }

            VectorUtils.TessellatePath(
                new BezierContour
                {
                    Segments = segments.ToArray(),
                    Closed = false
                },
                GetPathProperties(lineThickness, color),
                GetTessellationOptions(),
                out var vertices,
                out var indices
            );

            AddUiVertexes(vertices, indices, color);
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

        private void AddUiVertexes(Vector2[] vertices, ushort[] indices, Color vertColor, float z = 0)
        {
            var vertexCount = _vertices.Count;
            foreach (var vert in vertices)
                _vertices.Add(new UIVertex
                {
                    position = new float3(vert, z),
                    uv0 = Vector2.zero,
                    color = vertColor
                });

            foreach (var idx in indices) _indices.Add(idx + vertexCount);
        }
    }
}