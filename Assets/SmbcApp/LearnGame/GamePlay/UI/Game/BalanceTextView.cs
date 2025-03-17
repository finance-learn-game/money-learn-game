using R3;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.GamePlay.GamePlayObjects.RuntimeDataContainers;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace SmbcApp.LearnGame.GamePlay.UI.Game
{
    internal sealed class BalanceTextView : MonoBehaviour
    {
        [SerializeField] [Required] private TMP_Text balanceText;

        [Inject] internal PersistantPlayerRuntimeCollection PlayerCollection;

        private void Start()
        {
            var clientId = NetworkManager.Singleton.LocalClientId;
            if (PlayerCollection.TryGetPlayer(clientId, out var player))
                player.BalanceState.OnChangeAsObservable()
                    .Subscribe(balance => balanceText.text = $"所持金：￥{balance}")
                    .AddTo(gameObject);
        }
    }
}