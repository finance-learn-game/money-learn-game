using Sirenix.OdinInspector;
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

        [Inject]
        internal void Construct(IObjectResolver resolver)
        {
            resolver.Inject(stockList);
        }
    }
}