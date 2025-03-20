using System;
using Cysharp.Threading.Tasks;
using MessagePipe;
using R3;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.ConnectionManagement;
using SmbcApp.LearnGame.UnityService.Auth;
using SmbcApp.LearnGame.UnityService.Session;
using SmbcApp.LearnGame.Utils;
using Unity.Logging;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace SmbcApp.LearnGame.Gameplay.UI.MainMenu
{
    internal sealed class MainMenuUIMediator : MonoBehaviour
    {
        [SerializeField] [Required] private Image loadingBackdrop;

        [Inject] internal AuthenticationServiceFacade AuthServiceFacade;
        [Inject] internal ConnectionManager ConnectionManager;
        [Inject] internal ISubscriber<ConnectStatus> ConnectStatusSubscriber;
        [Inject] internal ProfileManager ProfileManager;
        [Inject] internal IObjectResolver Resolver;
        [Inject] internal SessionServiceFacade SessionServiceFacade;

        private void Start()
        {
            ConnectStatusSubscriber.Subscribe(OnConnectStatus).AddTo(gameObject);
        }

        private async UniTask Signin(string profile = null)
        {
            await UniTask.WaitUntil(() => AuthServiceFacade.IsInitialized);

            try
            {
                await AuthServiceFacade.SignIn(profile);
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to sign in");
                throw;
            }
        }

        public async UniTask JoinSessionWithCode(string sessionCode)
        {
            BlockUIWhileLoadingIsInProgress();

            await Signin();
            await ConnectionManager.StartClientSession(ProfileManager.CurrentProfile.CurrentValue, sessionCode);

            UnblockUIAfterLoadingIsComplete();
        }

        public async UniTask CreateSessionRequest()
        {
            BlockUIWhileLoadingIsInProgress();

            await Signin("Server");
            await ConnectionManager.StartServerSession("Server");

            UnblockUIAfterLoadingIsComplete();
        }

        private void BlockUIWhileLoadingIsInProgress()
        {
            loadingBackdrop.gameObject.SetActive(true);
        }

        private void UnblockUIAfterLoadingIsComplete()
        {
            loadingBackdrop.gameObject.SetActive(false);
        }

        private void OnConnectStatus(ConnectStatus status)
        {
            if (status is ConnectStatus.GenericDisconnect or ConnectStatus.StartClientFailed)
                UnblockUIAfterLoadingIsComplete();
        }
    }
}