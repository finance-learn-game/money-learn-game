using R3;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.GamePlay.GameState.NetworkData;
using SmbcApp.LearnGame.UIWidgets.Button;
using Unity.Logging;
using Unity.Netcode;
using UnityEngine;
using UnityScreenNavigator.Runtime.Core.Page;
using uPalette.Generated;
using VContainer;

namespace SmbcApp.LearnGame.GamePlay.UI.AvatarSelect
{
    internal sealed class AvatarSelectPageView : Page
    {
        [SerializeField] [Required] private SessionCodeView sessionCodeView;
        [SerializeField] [Required] private PlayerListView playerListView;
        [SerializeField] [Required] private AvatarListView avatarListView;
        [SerializeField] [Required] private UIButton readyButton;
        [SerializeField] [Required] private UIButton leaveButton;

        private bool _isReady;

        [Inject] internal NetworkAvatarSelection NetworkAvatarSelection;
        [Inject] internal IObjectResolver Resolver;

        private void Start()
        {
            Resolver.Inject(sessionCodeView);
            Resolver.Inject(playerListView);
            Resolver.Inject(avatarListView);

            var networkManager = NetworkManager.Singleton;
            if (!networkManager.IsServer)
            {
                readyButton.OnClick.Subscribe(OnReady).AddTo(gameObject);
                SetReadyButtonColors();
            }
            else
            {
                // サーバー側では不必要なものを表示しない
                readyButton.gameObject.SetActive(false);
                avatarListView.gameObject.SetActive(false);
            }

            leaveButton.OnClick.Subscribe(OnLeave).AddTo(gameObject);
        }

        private void OnReady(Unit _)
        {
            if (NetworkAvatarSelection.IsAvatarSelectFinished.Value)
            {
                Log.Warning("Avatar select is already finished.");
                return;
            }

            _isReady = !_isReady;
            SetReadyButtonColors();

            var clientId = NetworkManager.Singleton.LocalClientId;
            NetworkAvatarSelection.ServerChangeReadyStateRpc(clientId, _isReady);
        }

        private static void OnLeave(Unit _)
        {
            NetworkManager.Singleton.Shutdown();
        }

        private void SetReadyButtonColors()
        {
            readyButton.SetColor(_isReady ? ColorEntry.primaryFixed : ColorEntry.errorContainer);
            readyButton.SetTextColor(_isReady ? ColorEntry.onPrimaryFixed : ColorEntry.onErrorContainer);
            readyButton.Text = _isReady ? "準備OK" : "準備中";
        }
    }
}