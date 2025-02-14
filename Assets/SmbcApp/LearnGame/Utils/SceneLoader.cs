using Sirenix.OdinInspector;
using Unity.Logging;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SmbcApp.LearnGame.Utils
{
    /// <summary>
    ///     ゲーム内で使用するシーン名の定数
    /// </summary>
    public static class AppScenes
    {
        public const string StartUp = "StartUp";
        public const string MainMenu = "MainMenu";
        public const string AvatarSelect = "AvatarSelect";
        public const string GameScene = "Game";
        public const string Result = "Result";
    }

    public sealed class SceneLoader : NetworkBehaviour
    {
        [SerializeField] [Required] private ClientLoadingScreen clientLoadingScreen;
        [SerializeField] [Required] private LoadingProgressManager loadingProgressManager;

        private bool _isInitialized;

        private bool IsNetworkSceneManagementEnabled =>
            NetworkManager != null
            && NetworkManager.SceneManager != null
            && NetworkManager.NetworkConfig.EnableSceneManagement;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            NetworkManager.OnServerStarted += OnNetworkingSessionStarted;
            NetworkManager.OnClientStarted += OnNetworkingSessionStarted;
            NetworkManager.OnServerStopped += OnNetworkingSessionEnded;
            NetworkManager.OnClientStopped += OnNetworkingSessionEnded;
        }

        public override void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (NetworkManager != null)
            {
                NetworkManager.OnServerStarted -= OnNetworkingSessionStarted;
                NetworkManager.OnClientStarted -= OnNetworkingSessionStarted;
                NetworkManager.OnServerStopped -= OnNetworkingSessionEnded;
                NetworkManager.OnClientStopped -= OnNetworkingSessionEnded;
            }

            base.OnDestroy();
        }

        private void OnNetworkingSessionStarted()
        {
            if (!_isInitialized) return;

            if (IsNetworkSceneManagementEnabled)
                NetworkManager.SceneManager.OnSceneEvent += OnSceneEvent;
            _isInitialized = true;
        }

        private void OnNetworkingSessionEnded(bool unused)
        {
            if (!_isInitialized) return;

            if (IsNetworkSceneManagementEnabled)
                NetworkManager.SceneManager.OnSceneEvent -= OnSceneEvent;
            _isInitialized = false;
        }

        public void LoadScene(
            string sceneName,
            bool useNetworkSceneManager = false,
            LoadSceneMode loadSceneMode = LoadSceneMode.Single
        )
        {
            if (useNetworkSceneManager)
            {
                if (
                    IsSpawned
                    && IsNetworkSceneManagementEnabled
                    && !NetworkManager.ShutdownInProgress
                    && NetworkManager.IsServer
                )
                {
                    Log.Info("Loading scene: {0} (Network)", sceneName);
                    NetworkManager.SceneManager.LoadScene(sceneName, loadSceneMode);
                }
                else
                {
                    Log.Warning("Not loading scene: {0} (Network)", sceneName);
                }
            }
            else
            {
                Log.Info("Loading scene: {0} (Local)", sceneName);
                var loadOperation = SceneManager.LoadSceneAsync(sceneName, loadSceneMode);
                if (loadSceneMode != LoadSceneMode.Single) return;
                clientLoadingScreen.StartLoadingScreen(sceneName);
                loadingProgressManager.LocalLoadOperation = loadOperation;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (!IsSpawned || NetworkManager.ShutdownInProgress) clientLoadingScreen.StopLoadingScreen();
        }

        private void OnSceneEvent(SceneEvent sceneEvent)
        {
            switch (sceneEvent.SceneEventType)
            {
                // サーバーがクライアントにシーンをロードするよう指示
                case SceneEventType.Load:
                    if (NetworkManager.IsClient)
                    {
                        if (sceneEvent.LoadSceneMode == LoadSceneMode.Single)
                            clientLoadingScreen.StartLoadingScreen(sceneEvent.SceneName);
                        else
                            clientLoadingScreen.UpdateLoadingScreen(sceneEvent.SceneName);

                        loadingProgressManager.LocalLoadOperation = sceneEvent.AsyncOperation;
                    }

                    break;
                case SceneEventType.LoadEventCompleted:
                    if (NetworkManager.IsClient)
                        clientLoadingScreen.StopLoadingScreen();
                    break;
                case SceneEventType.Synchronize:
                    if (
                        NetworkManager.IsClient
                        && !NetworkManager.IsHost
                        && NetworkManager.SceneManager.ClientSynchronizationMode == LoadSceneMode.Single
                    )
                        UnloadAdditiveScenes();
                    break;
                case SceneEventType.SynchronizeComplete:
                    if (NetworkManager.IsServer)
                        ClientStopLoadingScreenRpc(RpcTarget.Group(new[] { sceneEvent.ClientId }, RpcTargetUse.Temp));
                    break;
                case SceneEventType.Unload:
                case SceneEventType.ReSynchronize:
                case SceneEventType.UnloadEventCompleted:
                case SceneEventType.LoadComplete:
                case SceneEventType.UnloadComplete:
                case SceneEventType.ActiveSceneChanged:
                case SceneEventType.ObjectSceneChanged:
                default:
                    break;
            }
        }

        private static void UnloadAdditiveScenes()
        {
            var activeScene = SceneManager.GetActiveScene();
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded && scene != activeScene) SceneManager.UnloadSceneAsync(scene);
            }
        }

        [Rpc(SendTo.SpecifiedInParams)]
        private void ClientStopLoadingScreenRpc(RpcParams clientRpcParams = default)
        {
            clientLoadingScreen.StopLoadingScreen();
        }
    }
}