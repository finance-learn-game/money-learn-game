using Cysharp.Threading.Tasks;
using MessagePipe;
using R3;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.ApplicationLifecycle.Messages;
using SmbcApp.LearnGame.ConnectionManagement;
using SmbcApp.LearnGame.Data;
using SmbcApp.LearnGame.UnityService.Auth;
using SmbcApp.LearnGame.UnityService.Infrastructure.Messages;
using SmbcApp.LearnGame.UnityService.Session;
using SmbcApp.LearnGame.Utils;
using Unity.Logging;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SmbcApp.LearnGame.ApplicationLifecycle
{
    internal sealed class ApplicationController : LifetimeScope
    {
        [SerializeField] [Required] private NetworkManager networkManager;
        [SerializeField] [Required] private ConnectionManager connectionManager;
        [SerializeField] [Required] private SceneLoader sceneLoader;

        private SaveDataManager _saveDataManager;

        private void Start()
        {
            Container
                .Resolve<ISubscriber<QuitApplicationMessage>>()
                .Subscribe(QuitGame)
                .AddTo(gameObject);

            Application.wantsToQuit += OnWantToQuit;
            DontDestroyOnLoad(gameObject);
            sceneLoader.LoadScene(AppScenes.MainMenu);
        }

        private async UniTask LeaveBeforeQuit()
        {
            await _saveDataManager.DisposeAsync();

            await UniTask.Yield();
            QuitGame();
        }

        private bool OnWantToQuit()
        {
            Application.wantsToQuit -= OnWantToQuit;

            var canQuit = !_saveDataManager.IsDirty;
            if (!canQuit) LeaveBeforeQuit().Forget();

            return canQuit;
        }

        private static void QuitGame(QuitApplicationMessage _ = default)
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(networkManager);
            builder.RegisterComponent(connectionManager);
            builder.RegisterComponent(sceneLoader);

            var option = builder.RegisterMessagePipe();
            builder.RegisterBuildCallback(c =>
            {
                GlobalMessagePipe.SetProvider(c.AsServiceProvider());
                _saveDataManager = c.Resolve<SaveDataManager>();

                c.Resolve<ISubscriber<UnityServiceErrorMessage>>()
                    .Subscribe(msg => Log.Error(msg.Message))
                    .AddTo(gameObject);
            });

            builder.RegisterMessageBroker<UnityServiceErrorMessage>(option);
            builder.RegisterMessageBroker<ConnectStatus>(option);
            builder.RegisterMessageBroker<ConnectionEventMessage>(option);

            builder.Register<SessionServiceFacade>(Lifetime.Singleton);
            builder.UseEntryPoints(c =>
            {
                c.Add<ProfileManager>().AsSelf();
                c.Add<SaveDataManager>().AsSelf();
                c.Add<AuthenticationServiceFacade>().AsSelf();
            });
        }
    }
}