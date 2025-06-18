using MessagePipe;
using R3;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.ApplicationLifecycle.Messages;
using SmbcApp.LearnGame.Gameplay.UI.MainMenu;
using SmbcApp.LearnGame.UIWidgets.Button;
using SmbcApp.LearnGame.UIWidgets.Modal;
using UnityEngine;
using UnityScreenNavigator.Runtime.Core.Modal;
using UnityScreenNavigator.Runtime.Core.Page;
using VContainer;

namespace SmbcApp.LearnGame.GamePlay.UI.MainMenu
{
    internal sealed class MainMenuPageView : Page
    {
        [SerializeField] [Required] private UIButton startServerButton;
        [SerializeField] [Required] private UIButton joinSessionButton;
        [SerializeField] [Required] private UIButton quitButton;
        [SerializeField] [Required] private UIModal.Ref profileModal;

        [Inject] internal MainMenuUIMediator MainMenuUIMediator;
        [Inject] internal ModalContainer ModalContainer;
        [Inject] internal IPublisher<QuitApplicationMessage> QuitApplicationPublisher;

        private void Start()
        {
#if UNITY_WEBGL
            startServerButton.IsInteractable = false;
#endif

            joinSessionButton.OnClick
                .Subscribe(_ => ModalContainer.Push(profileModal.AssetGUID, true))
                .AddTo(gameObject);
            startServerButton.OnClick
                .SubscribeAwait((_, _) => MainMenuUIMediator.CreateSessionRequest())
                .AddTo(gameObject);
            quitButton.OnClick
                .Subscribe(_ => QuitApplicationPublisher.Publish(new QuitApplicationMessage()))
                .AddTo(gameObject);
        }
    }
}