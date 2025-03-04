using System;
using SmbcApp.LearnGame.ConnectionManagement.ConnectionState;
using Unity.Logging;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace SmbcApp.LearnGame.ConnectionManagement
{
    public enum ConnectStatus
    {
        Undefined,
        Success, // 接続成功
        ServerFull, // サーバーが満員
        LoggedInAgain, // 二重ログイン
        UserRequestedDisconnect, // ユーザーによる切断
        GenericDisconnect, // サーバーからの切断
        Reconnecting, // 再接続中
        IncompatibleBuildType, // クライアントのビルドタイプがサーバーと互換性がない
        HostEndedSession, // ホストがセッションを終了
        StartServerFailed, // サーバーのバインドに失敗
        StartClientFailed // サーバーへの接続に失敗
    }

    public struct ConnectionEventMessage
    {
        public ConnectStatus ConnectStatus;
        public string PlayerName;
    }

    [Serializable]
    internal sealed class ConnectionPayload
    {
        public string playerId;
        public string playerName;
        public bool isDebug;
    }

    /// <summary>
    ///     ネットワークマネージャーを介した接続を処理するステートマシン
    /// </summary>
    public sealed class ConnectionManager : MonoBehaviour
    {
        private ConnectionState.ConnectionState _currentState;

        [Inject] internal NetworkManager NetworkManager;
        [Inject] internal IObjectResolver Resolver;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            var states = new ConnectionState.ConnectionState[]
            {
                Offline,
                ClientConnecting,
                ClientConnected,
                StartingServer,
                Hosting
            };
            foreach (var connectionState in states)
                Resolver.Inject(connectionState);

            _currentState = Offline;

            NetworkManager.NetworkConfig.ConnectionApproval = true;
            NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
            NetworkManager.OnClientDisconnectCallback += OnClientDisconnectCallback;
            NetworkManager.OnServerStarted += OnServerStarted;
            NetworkManager.ConnectionApprovalCallback += ApprovalCheck;
            NetworkManager.OnTransportFailure += OnTransportFailure;
            NetworkManager.OnServerStopped += OnServerStopped;
        }

        private void OnDestroy()
        {
            NetworkManager.OnClientConnectedCallback -= OnClientConnectedCallback;
            NetworkManager.OnClientDisconnectCallback -= OnClientDisconnectCallback;
            NetworkManager.OnServerStarted -= OnServerStarted;
            NetworkManager.ConnectionApprovalCallback -= ApprovalCheck;
            NetworkManager.OnTransportFailure -= OnTransportFailure;
            NetworkManager.OnServerStopped -= OnServerStopped;
        }

        internal void ChangeState(ConnectionState.ConnectionState nextState)
        {
            Log.Info("{0}: Changing state from {1} to {2}", name, _currentState.GetType().Name,
                nextState.GetType().Name);

            _currentState?.Exit();
            _currentState = nextState;
            _currentState.Enter();
        }

        #region Event Callbacks

        private void OnClientDisconnectCallback(ulong clientId)
        {
            _currentState.OnClientDisconnect(clientId);
        }

        private void OnClientConnectedCallback(ulong clientId)
        {
            _currentState.OnClientConnected(clientId);
        }

        private void OnServerStarted()
        {
            _currentState.OnServerStarted();
        }

        private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request,
            NetworkManager.ConnectionApprovalResponse response)
        {
            _currentState.ApprovalCheck(request, response);
        }

        private void OnTransportFailure()
        {
            _currentState.OnTransportFailure();
        }

        private void OnServerStopped(bool _)
        {
            _currentState.OnServerStopped();
        }

        public void StartClientSession(string playerName)
        {
            _currentState.StartClientSession(playerName);
        }

        public void StartServerSession(string playerName)
        {
            _currentState.StartServerSession(playerName);
        }

        public void RequestShutdown()
        {
            _currentState.OnUserRequestedShutdown();
        }

        #endregion

        #region States

        internal readonly OfflineState Offline = new();
        internal readonly ClientConnectingState ClientConnecting = new();
        internal readonly ClientConnectedState ClientConnected = new();
        internal readonly StartingServerState StartingServer = new();
        internal readonly HostingState Hosting = new();

        #endregion
    }
}