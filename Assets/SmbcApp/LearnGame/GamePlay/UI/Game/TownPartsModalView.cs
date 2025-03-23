using Sirenix.OdinInspector;
using SmbcApp.LearnGame.UIWidgets.Modal;
using UnityEngine;
using VContainer;

namespace SmbcApp.LearnGame.GamePlay.UI.Game
{
    internal sealed class TownPartsModalView : UIModal
    {
        [SerializeField] [Required] private TownPartsListView townPartsListView;
        [SerializeField] [Required] private TownPartsPointView townPartsPointView;
        [SerializeField] [Required] private BalanceTextView balanceTextView;

        [Inject]
        internal void Construct(IObjectResolver resolver)
        {
            resolver.Inject(townPartsListView);
            resolver.Inject(townPartsPointView);
            resolver.Inject(balanceTextView);
        }
    }
}