using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using R3;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SmbcApp.LearnGame.UIWidgets.Chart
{
    [RequireComponent(typeof(ChartGraphic))]
    internal sealed class UIChart : MonoBehaviour
    {
        [SerializeField] [Required] private ChartGraphic chartGraphic;
        [SerializeField] [Required] private UIChartValueLabel.Ref valueLabelPrefab;

        private readonly Stack<UIChartValueLabel> _valueLabels = new();
        private AsyncObjectPool<UIChartValueLabel> _valueLabelPool;

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

        private void OnDataChanged(ChartGraphic.ChartData[] _)
        {
            SetYValueLabel().Forget();
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
                var labelTrans = (RectTransform)label.transform;

                labelTrans.anchoredPosition = new Vector2(-labelTrans.rect.width - 10, localYPos);
                var value = i * chartGraphic.GridStep + chartGraphic.YDataRange.x;
                label.Value = value.ToString("F0");
                _valueLabels.Push(label);
            }
        }
    }
}