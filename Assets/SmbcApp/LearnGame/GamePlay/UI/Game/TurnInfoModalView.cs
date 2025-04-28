using Sirenix.OdinInspector;
using SmbcApp.LearnGame.GamePlay.GamePlayObjects;
using SmbcApp.LearnGame.GamePlay.GamePlayObjects.RuntimeDataContainers;
using SmbcApp.LearnGame.GamePlay.GameState.NetworkData;
using SmbcApp.LearnGame.UIWidgets.Modal;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace SmbcApp.LearnGame.GamePlay.UI.Game
{
    internal sealed class TurnInfoModalView : UIModal
    {
        [SerializeField] [Required] private TMP_Text dateText;
        [SerializeField] [Required] private TMP_Text salaryText;
        [SerializeField] [Required] private TMP_Text titleText;
        [SerializeField] [Required] private NewsListView newsListView;

        private NetworkGameTurn _gameTurn;
        private PersistantPlayer _player;

        protected override void Start()
        {
            base.Start();

            var date = _gameTurn.CurrentTime;
            dateText.text = $"{date:yyyy/MM} になりました";
            salaryText.text = $"{_player.BalanceState.GaveSalary} 円の給料を受け取りました";
            titleText.text = $"{date:yyyy/MM} に起きるニュース情報";
        }

        [Inject]
        internal void Construct(
            IObjectResolver resolver,
            NetworkGameTurn gameTurn,
            PersistantPlayerRuntimeCollection playerCollection
        )
        {
            _gameTurn = gameTurn;
            playerCollection.TryGetPlayer(NetworkManager.Singleton.LocalClientId, out _player);
            resolver.Inject(newsListView);
        }
    }
}