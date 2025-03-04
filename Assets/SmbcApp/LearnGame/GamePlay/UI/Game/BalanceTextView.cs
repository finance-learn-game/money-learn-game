using R3;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.GamePlay.GamePlayObjects.RuntimeDataContainers;
using TMPro;
using Unity.Logging;
using Unity.Netcode;
using UnityEngine;

namespace SmbcApp.LearnGame.GamePlay.UI.Game
{
    internal sealed class BalanceTextView : MonoBehaviour
    {
        [SerializeField] [Required] private TMP_Text balanceText;
        [SerializeField] [Required] private PersistantPlayerRuntimeCollection playerCollection;

        private void Start()
        {
            var clientId = NetworkManager.Singleton.LocalClientId;
            if (playerCollection.TryGetPlayer(clientId, out var player))
                player.BalanceState.OnChangeAsObservable()
                    .Subscribe(balance => balanceText.text = $"￥{balance}")
                    .AddTo(gameObject);
            else
                Log.Error("Failed to get player object.");
        }
    }
}