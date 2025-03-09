using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using R3;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.UIWidgets.Button;
using SmbcApp.LearnGame.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SmbcApp.LearnGame.UIWidgets.Chart
{
    internal sealed class ChartLegend : MonoBehaviour
    {
        [SerializeField] [Required] private ChartGraphic chartGraphic;
        [SerializeField] [Required] private UIButton minimizeButton;
        [SerializeField] [Required] private Sprite minimizeIcon;
        [SerializeField] [Required] private Sprite maximizeIcon;
        [SerializeField] [Required] private ChartLegendItem.Ref legendItemPrefab;

        private readonly Stack<ChartLegendItem> _items = new();
        private bool _minimized;

        private void Start()
        {
            HandleMinimizeButton();
            HandleOnDataChanged();
        }

        private void HandleOnDataChanged()
        {
            var pool = new AsyncObjectPool<ChartLegendItem>(
                () => legendItemPrefab.InstantiateAsync(transform).ToUniTask(),
                item => item.gameObject.SetActive(true),
                item => item.gameObject.SetActive(false),
                item => Addressables.ReleaseInstance(item.gameObject)
            );
            chartGraphic.OnDataChanged.SubscribeAwait(async (dataArr, _) =>
            {
                while (_items.TryPop(out var item)) pool.Return(item);
                foreach (var (data, i) in dataArr.Select((data, i) => (data, i)))
                {
                    var item = await pool.Rent();
                    item.Configure(data.label, data.color);
                    item.gameObject.SetActive(!_minimized);
                    item.IsShow = data.show;
                    item.OnToggle
                        .Subscribe(isShow => chartGraphic.SetChartShow(i, isShow))
                        .AddTo(item);
                    _items.Push(item);
                }
            }).AddTo(gameObject);
        }

        private void HandleMinimizeButton()
        {
            SetMinimized(false);
            minimizeButton.OnClick
                .Subscribe(_ => SetMinimized(!_minimized))
                .AddTo(gameObject);

            return;

            void SetMinimized(bool value)
            {
                _minimized = value;
                foreach (var item in _items) item.gameObject.SetActive(!_minimized);
                minimizeButton.Image.sprite = _minimized ? maximizeIcon : minimizeIcon;
            }
        }
    }
}