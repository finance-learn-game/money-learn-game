using System;
using R3;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.UIWidgets.Button;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using uPalette.Generated;
using uPalette.Runtime.Core.Synchronizer.Color;

namespace SmbcApp.LearnGame.GamePlay.UI.Game
{
    internal sealed class StockListItemView : MonoBehaviour
    {
        [SerializeField] [Required] private TMP_Text stockNameText;
        [SerializeField] [Required] private TMP_Text stockPriceText;
        [SerializeField] [Required] private TMP_Text stockCountText;
        [SerializeField] [Required] private TMP_Text fluctuationText;
        [SerializeField] [Required] private GraphicColorSynchronizer fluctuationColor;
        [SerializeField] [Required] private UIButton buyButton;
        [SerializeField] [Required] private UIButton sellButton;

        public Observable<Unit> OnBuyButtonClicked => buyButton.OnClick;
        public Observable<Unit> OnSellButtonClicked => sellButton.OnClick;

        public void Configure(string stockName, int stockPrice, int stockCount, int fluctuation)
        {
            stockNameText.text = stockName;
            stockPriceText.text = $"￥{stockPrice}";
            stockCountText.text = $"保有株式数: {stockCount}";
            fluctuationText.text = $"{(fluctuation > 0 ? "+" : "-")}￥{math.abs(fluctuation)} (前月比)";
            fluctuationColor.SetEntryId((fluctuation > 0 ? ColorEntry.surfaceTint : ColorEntry.error)
                .ToEntryId());
        }

        [Serializable]
        public class Ref : ComponentReference<StockListItemView>
        {
            public Ref(string guid) : base(guid)
            {
            }
        }
    }
}