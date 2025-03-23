using R3;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.GamePlay.GamePlayObjects.RuntimeDataContainers;
using TMPro;
using Unity.Logging;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace SmbcApp.LearnGame.GamePlay.UI.Game
{
    internal sealed class TownPartsPointView : MonoBehaviour
    {
        [SerializeField] [Required] private TMP_Text pointText;

        [Inject] internal PersistantPlayerRuntimeCollection PlayerCollection;

        private void Start()
        {
            var clientId = NetworkManager.Singleton.LocalClientId;
            if (!PlayerCollection.TryGetPlayer(clientId, out var player))
            {
                Log.Error("Player not found (clientId: {0})", clientId);
                return;
            }

            player.TownPartsState.OnChangeCurrentPoint
                .Subscribe(p => pointText.text = $"現在のポイント：{p} pt")
                .AddTo(this);
        }
    }
}