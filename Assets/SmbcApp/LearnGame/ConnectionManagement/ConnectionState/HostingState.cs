using System.Text;
using Cysharp.Threading.Tasks;
using MessagePipe;
using SmbcApp.LearnGame.Infrastructure;
using SmbcApp.LearnGame.UnityService.Session;
using SmbcApp.LearnGame.Utils;
using Unity.Logging;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace SmbcApp.LearnGame.ConnectionManagement.ConnectionState
{
    internal sealed class HostingState : OnlineState
    {
        private const int MaxConnectPayload = 1024;
        [Inject] internal IPublisher<ConnectionEventMessage> ConnectionEventPublisher;
        [Inject] internal SceneLoader SceneLoader;
        [Inject] internal SessionServiceFacade SessionServiceFacade;

        public override UniTask Enter()
        {
            // TODO: AvatarSelectSceneをロードする
            return UniTask.CompletedTask;
        }

        public override UniTask Exit()
        {
            SessionManager<SessionPlayerData>.Instance.OnServerEnded();
            return UniTask.CompletedTask;
        }

        public override void OnClientConnected(ulong clientId)
        {
            if (ConnectionManager.NetworkManager.IsServer)
            {
                Log.Info("Server connected to client {0}", clientId);
                return;
            }

            // ApprovalCheckで設定したデータを取得
            var playerData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(clientId);
            if (playerData != null)
            {
                ConnectionEventPublisher.Publish(new ConnectionEventMessage
                {
                    ConnectStatus = ConnectStatus.Success,
                    PlayerName = playerData.Value.PlayerName
                });
            }
            else
            {
                Log.Error("No player data associated with client {0}", clientId);
                var reason = JsonUtility.ToJson(ConnectStatus.GenericDisconnect);
                ConnectionManager.NetworkManager.DisconnectClient(clientId, reason);
            }
        }

        public override void OnClientDisconnect(ulong clientId)
        {
            if (ConnectionManager.NetworkManager.IsServer)
            {
                Log.Info("Server disconnected from client {0}", clientId);
                return;
            }

            if (clientId == ConnectionManager.NetworkManager.LocalClientId) return;

            var sessionManager = SessionManager<SessionPlayerData>.Instance;
            var playerId = sessionManager.GetPlayerId(clientId);
            if (playerId == null) return;

            var sessionData = sessionManager.GetPlayerData(clientId);
            if (sessionData.HasValue)
                ConnectionEventPublisher.Publish(new ConnectionEventMessage
                {
                    ConnectStatus = ConnectStatus.GenericDisconnect,
                    PlayerName = sessionData.Value.PlayerName
                });
            sessionManager.DisconnectClient(clientId);
        }

        public override void OnUserRequestedShutdown()
        {
            var reason = JsonUtility.ToJson(ConnectStatus.HostEndedSession);
            var connectedClientsIds = ConnectionManager.NetworkManager.ConnectedClientsIds;
            for (var i = connectedClientsIds.Count - 1; i >= 0; i--)
            {
                var id = connectedClientsIds[i];
                if (id != ConnectionManager.NetworkManager.LocalClientId)
                    ConnectionManager.NetworkManager.DisconnectClient(id, reason);
            }

            ConnectionManager.ChangeState(ConnectionManager.Offline);
        }

        public override void OnServerStopped()
        {
            ConnectStatusPublisher.Publish(ConnectStatus.GenericDisconnect);
            ConnectionManager.ChangeState(ConnectionManager.Offline);
        }

        public override async UniTask ApprovalCheck(
            NetworkManager.ConnectionApprovalRequest request,
            NetworkManager.ConnectionApprovalResponse response
        )
        {
            var connectionData = request.Payload;
            var clientId = request.ClientNetworkId;
            
            // ペイロードが大きすぎる場合は接続を拒否する (不正な接続)
            if (connectionData.Length > MaxConnectPayload)
            {
                response.Approved = false;
                return;
            }

            var payload = Encoding.UTF8.GetString(connectionData);
            var connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload);
            var gameReturnStatus = GetConnectStatus(connectionPayload);

            if (gameReturnStatus == ConnectStatus.Success)
            {
                SessionManager<SessionPlayerData>.Instance.SetupConnectingPlayerSessionData(
                    clientId,
                    connectionPayload.playerId,
                    new SessionPlayerData(clientId, connectionPayload.playerName, new NetworkGuid(), 0, true)
                );

                response.Approved = true;
                response.CreatePlayerObject = true;
                response.Position = Vector3.zero;
                response.Rotation = Quaternion.identity;
                return;
            }

            response.Approved = false;
            response.Reason = JsonUtility.ToJson(gameReturnStatus);
            if (SessionServiceFacade.CurrentSession != null)
                await SessionServiceFacade.RemovePlayerFromSession(connectionPayload.playerId);
        }

        private ConnectStatus GetConnectStatus(ConnectionPayload payload)
        {
            if (ConnectionManager.NetworkManager.ConnectedClientsIds.Count >= ConnectionManager.MaxConnectionPlayers)
                return ConnectStatus.ServerFull;

            if (payload.isDebug != Debug.isDebugBuild) return ConnectStatus.IncompatibleBuildType;
            return SessionManager<SessionPlayerData>.Instance.IsDuplicateConnection(payload.playerId)
                ? ConnectStatus.LoggedInAgain
                : ConnectStatus.Success;
        }
    }
}