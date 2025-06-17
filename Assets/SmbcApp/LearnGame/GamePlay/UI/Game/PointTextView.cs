using R3;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.GamePlay.GamePlayObjects.RuntimeDataContainers;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace SmbcApp.LearnGame.GamePlay.UI.Game
{
    internal sealed class PointTextView : MonoBehaviour
    {
        [SerializeField] [Required] private TMP_Text pointText;

        [Inject] internal PersistantPlayerRuntimeCollection PlayerCollection;

        private void Start()
        {
            var clientId = NetworkManager.Singleton.LocalClientId;
            if (PlayerCollection.TryGetPlayer(clientId, out var player))
                player.TownPartsState.OnChanged
                    .Select(_ => player.TownPartsState.CurrentPoint)
                    .Prepend(0)
                    .Subscribe(point => pointText.text = $"ポイント：￥{point}")
                    .AddTo(gameObject);
        }
    }
}