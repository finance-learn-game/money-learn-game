using Sirenix.OdinInspector;
using SmbcApp.LearnGame.GamePlay.GameState.NetworkData;
using SmbcApp.LearnGame.UIWidgets.Modal;
using TMPro;
using UnityEngine;
using VContainer;

namespace SmbcApp.LearnGame.GamePlay.UI.Game
{
    internal sealed class TurnInfoModalView : UIModal
    {
        [SerializeField] [Required] private TMP_Text dateText;
        [SerializeField] [Required] private TMP_Text salaryText;
        [SerializeField] [Required] private NewsListView newsListView;

        private NetworkGameTurn _gameTurn;

        protected override void Start()
        {
            base.Start();

            var date = _gameTurn.CurrentTime;
            dateText.text = $"{date:yyyy/MM/dd} になりました";
        }

        [Inject]
        internal void Construct(IObjectResolver resolver, NetworkGameTurn gameTurn)
        {
            _gameTurn = gameTurn;
            resolver.Inject(newsListView);
        }
    }
}