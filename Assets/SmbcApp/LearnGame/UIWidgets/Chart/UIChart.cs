using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using R3;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SmbcApp.LearnGame.UIWidgets.Chart
{
    [RequireComponent(typeof(ChartGraphic))]
    public sealed class UIChart : MonoBehaviour
    {
        [SerializeField] [Required] private ChartGraphic chartGraphic;
        [SerializeField] [Required] private UIChartValueLabel.Ref valueLabelPrefab;

        private readonly Stack<UIChartValueLabel> _valueLabels = new();
        private readonly Stack<UIChartValueLabel> _xLabels = new();
        private AsyncObjectPool<UIChartValueLabel> _valueLabelPool;

        public ChartGraphic ChartGraphic => chartGraphic;

        private void Start()
        {
            _valueLabelPool = new AsyncObjectPool<UIChartValueLabel>(
                () => valueLabelPrefab.InstantiateAsync(chartGraphic.transform).ToUniTask(),
                label => label.gameObject.SetActive(true),
                label => label.gameObject.SetActive(false),
                label => Addressables.ReleaseInstance(label.gameObject)
            );

            chartGraphic.OnDataChanged.Subscribe(OnDataChanged).AddTo(gameObject);
        }

        private void OnDestroy()
        {
            _valueLabelPool.Dispose();
        }

        private void OnDataChanged(ChartGraphic.ChartData[] _)
        {
            SetYValueLabel().Forget();
        }

        public async UniTask SetXLabels(IEnumerable<string> labels)
        {
            while (_xLabels.TryPop(out var label)) _valueLabelPool.Return(label);

            var tasks = new List<UniTask>();
            foreach (var (localXPos, label) in chartGraphic.XLabelPositions.Zip(labels, (p, l) => (p, l)))
                tasks.Add(Put(localXPos, label));
            await UniTask.WhenAll(tasks);

            return;

            async UniTask Put(float localXPos, string label)
            {
                var xLabel = await _valueLabelPool.Rent();
                xLabel.TextAlignment = TextAlignmentOptions.Center;
                var xLabelTrans = (RectTransform)xLabel.transform;

                var localYPos = chartGraphic.YRange.x - xLabelTrans.rect.height - 10;
                xLabelTrans.anchoredPosition = new Vector2(localXPos - xLabelTrans.rect.width / 2, localYPos);
                xLabel.Value = label;
                _xLabels.Push(xLabel);
            }
        }

        private async UniTask SetYValueLabel()
        {
            while (_valueLabels.TryPop(out var label)) _valueLabelPool.Return(label);

            var tasks = new List<UniTask>();
            foreach (var (localYPos, i) in chartGraphic.YGridPositions.Select((f, i) => (f, i)))
                tasks.Add(Put(i, localYPos));
            await UniTask.WhenAll(tasks);

            return;

            async UniTask Put(int i, float localYPos)
            {
                var label = await _valueLabelPool.Rent();
                label.TextAlignment = TextAlignmentOptions.MidlineRight;
                var labelTrans = (RectTransform)label.transform;

                labelTrans.anchoredPosition = new Vector2(-labelTrans.rect.width - 20, localYPos);
                var value = i * chartGraphic.GridStep + chartGraphic.YDataRange.x;
                label.Value = value.ToString("F0");
                _valueLabels.Push(label);
            }
        }
    }
}