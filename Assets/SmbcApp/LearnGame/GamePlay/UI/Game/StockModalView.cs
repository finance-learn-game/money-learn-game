using R3;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.UIWidgets.Button;
using SmbcApp.LearnGame.UIWidgets.Modal;
using UnityEngine;
using VContainer;

namespace SmbcApp.LearnGame.GamePlay.UI.Game
{
    /// <summary>
    ///     保有株式の売買を行うモーダル
    /// </summary>
    internal sealed class StockModalView : UIModal
    {
        [SerializeField] [Required] private StockListView stockList;
        [SerializeField] [Required] private UIButton stockChartButton;
        [SerializeField] [Required] private Ref stockChartModal;

        protected override void Start()
        {
            base.Start();

            stockChartButton.OnClick
                .Subscribe(_ => ModalContainer.Push(stockChartModal.AssetGUID, true))
                .AddTo(gameObject);
        }

        [Inject]
        internal void Construct(IObjectResolver resolver)
        {
            resolver.Inject(stockList);
        }
    }
}