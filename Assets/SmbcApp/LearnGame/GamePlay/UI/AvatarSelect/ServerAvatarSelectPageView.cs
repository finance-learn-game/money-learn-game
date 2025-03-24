using R3;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.GamePlay.GameState.NetworkData;
using SmbcApp.LearnGame.UIWidgets.Button;
using SmbcApp.LearnGame.Utils;
using Unity.Netcode;
using UnityEngine;
using UnityScreenNavigator.Runtime.Core.Page;
using VContainer;

namespace SmbcApp.LearnGame.GamePlay.UI.AvatarSelect
{
    internal sealed class ServerAvatarSelectPageView : Page
    {
        [SerializeField] [Required] private SessionCodeView sessionCodeView;
        [SerializeField] [Required] private PlayerListView playerListView;
        [SerializeField] [Required] private UIButton startButton;
        [SerializeField] [Required] private UIButton leaveButton;

        private NetworkAvatarSelection _avatarSelection;

        private void Start()
        {
            leaveButton.OnClick.Subscribe(OnLeave).AddTo(gameObject);
            startButton.OnClick.Subscribe(OnGameStart).AddTo(gameObject);

            _avatarSelection.IsAvatarSelectFinished.OnChangedAsObservable()
                .Prepend(false)
                .Subscribe(finished => startButton.IsInteractable = finished)
                .AddTo(gameObject);
        }

        [Inject]
        internal void Construct(IObjectResolver resolver, NetworkAvatarSelection avatarSelection)
        {
            _avatarSelection = avatarSelection;

            resolver.Inject(sessionCodeView);
            resolver.Inject(playerListView);
        }

        private static void OnLeave(Unit _)
        {
            NetworkManager.Singleton.Shutdown();
        }

        private void OnGameStart(Unit _)
        {
            _avatarSelection.StartGameIfReady();
        }
    }
}