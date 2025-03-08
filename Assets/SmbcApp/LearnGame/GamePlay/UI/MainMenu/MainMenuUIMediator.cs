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

        public async UniTask<bool> JoinSessionWithCode(string sessionCode)
        {
            BlockUIWhileLoadingIsInProgress();

            await Signin();
            var res = await SessionServiceFacade.TryJoinSession(sessionCode);

            UnblockUIAfterLoadingIsComplete();

            if (res)
            {
                Log.Info("Joined session with code {0}", sessionCode);
                ConnectionManager.StartClientSession(ProfileManager.CurrentProfile.CurrentValue);
            }
            else
            {
                Log.Error("Failed to join session with code {0}", sessionCode);
            }

            return res;
        }

        public async UniTask CreateSessionRequest()
        {
            BlockUIWhileLoadingIsInProgress();

            await Signin("Server");
            var sessionCreationAttempt = await SessionServiceFacade.TryCreateSession();
            if (sessionCreationAttempt)
            {
                var session = SessionServiceFacade.CurrentSession;
                Log.Info("Created session with ID {0} and code {1}", session.Id, session.Code);
                ConnectionManager.StartServerSession("Server");
            }
            else
            {
                Log.Error("Failed to create session");
            }

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