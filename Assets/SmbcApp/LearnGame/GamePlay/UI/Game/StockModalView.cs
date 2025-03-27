using R3;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.GamePlay.GameState.NetworkData;
using SmbcApp.LearnGame.UIWidgets.Button;
using SmbcApp.LearnGame.UIWidgets.Modal;
using TMPro;
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
        [SerializeField] [Required] private BalanceTextView balanceTextView;
        [SerializeField] [Required] private Ref stockChartModal;
        [SerializeField] [Required] private TMP_Text titleText;

        private NetworkGameTurn _gameTurn;

        protected override void Start()
        {
            base.Start();

            stockChartButton.OnClick
                .Subscribe(_ => ModalContainer.Push(stockChartModal.AssetGUID, true))
                .AddTo(gameObject);
            titleText.text = $"月初営業日の終値での取引 ({_gameTurn.CurrentTime:yyyy/MM})";
        }

        [Inject]
        internal void Construct(IObjectResolver resolver, NetworkGameTurn gameTurn)
        {
            _gameTurn = gameTurn;

            resolver.Inject(stockList);
            resolver.Inject(balanceTextView);
        }
    }
}