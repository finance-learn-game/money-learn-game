using System;
using System.Text;
using Cysharp.Threading.Tasks;
using MessagePipe;
using SmbcApp.LearnGame.GamePlay.Configuration;
using SmbcApp.LearnGame.Infrastructure;
using SmbcApp.LearnGame.SceneLoader;
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
        [Inject] internal SceneLoader.SceneLoader SceneLoader;
        [Inject] internal SessionServiceFacade SessionServiceFacade;

        public override UniTask Enter()
        {
            SceneLoader.LoadScene(AppScenes.AvatarSelect, true);
            return UniTask.CompletedTask;
        }

        public override UniTask Exit()
        {
            SessionManager<SessionPlayerData>.Instance.OnServerEnded();
            return UniTask.CompletedTask;
        }

        public override void OnClientConnected(ulong clientId)
        {
            Log.Info("Client connected: {0}", clientId);
            if (clientId == ConnectionManager.NetworkManager.LocalClientId) return;

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
            Log.Info("Client disconnected: {0}", clientId);
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

            if (SessionServiceFacade.CurrentSession != null)
                SessionServiceFacade.RemovePlayerFromSession(playerId).Forget();
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

            ConnectionManager.ChangeState(ConnectionManager.Offline).Forget();
        }

        public override void OnServerStopped()
        {
            ConnectStatusPublisher.Publish(ConnectStatus.GenericDisconnect);
            ConnectionManager.ChangeState(ConnectionManager.Offline).Forget();
        }

        public override async UniTask ApprovalCheck(
            NetworkManager.ConnectionApprovalRequest request,
            NetworkManager.ConnectionApprovalResponse response
        )
        {
            try
            {
                var connectionData = request.Payload;
                var clientId = request.ClientNetworkId;

                // ペイロードが大きすぎる場合は接続を拒否する (不正な接続)
                if (connectionData.Length > MaxConnectPayload)
                {
                    response.Approved = false;
                    Log.Error("Connection refused for client {0} with MaxConnectPayload", clientId);
                    return;
                }

                var payload = Encoding.UTF8.GetString(connectionData);
                if (string.IsNullOrEmpty(payload))
                {
                    response.Approved = false;
                    Log.Error("Connection refused for client {0} with empty payload", clientId);
                    return;
                }

                var connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload);
                if (connectionPayload == null)
                {
                    response.Approved = false;
                    Log.Error("Connection refused for client {0} with invalid payload", clientId);
                    return;
                }

                var gameReturnStatus = GetConnectStatus(connectionPayload);

                if (gameReturnStatus == ConnectStatus.Success)
                {
                    SessionManager<SessionPlayerData>.Instance.SetupConnectingPlayerSessionData(
                        clientId,
                        connectionPayload.playerId,
                        new SessionPlayerData(
                            clientId,
                            connectionPayload.playerName,
                            new NetworkGuid(),
                            0,
                            Array.Empty<int>(),
                            Array.Empty<TownPartData>(),
                            true
                        )
                    );

                    Log.Info("Connection approved for client {0}", clientId);
                    response.Approved = true;
                    response.CreatePlayerObject = true;
                    response.Position = Vector3.zero;
                    response.Rotation = Quaternion.identity;
                    return;
                }

                response.Approved = false;
                response.Reason = JsonUtility.ToJson(gameReturnStatus);
                Log.Info("Connection refused for client {0} with reason {1}", clientId, gameReturnStatus);
                if (SessionServiceFacade.CurrentSession != null)
                    await SessionServiceFacade.RemovePlayerFromSession(connectionPayload.playerId);
            }
            catch (Exception e)
            {
                Log.Error(e, "Error in ApprovalCheck, see following exception");
                response.Approved = false;
                response.Reason = JsonUtility.ToJson(ConnectStatus.GenericDisconnect);
            }
        }

        private ConnectStatus GetConnectStatus(ConnectionPayload payload)
        {
            var maxConnectionPlayers = GameConfiguration.Instance.MaxPlayers;
            if (ConnectionManager.NetworkManager.ConnectedClientsIds.Count >= maxConnectionPlayers)
                return ConnectStatus.ServerFull;

            // if (payload.isDebug != Debug.isDebugBuild) return ConnectStatus.IncompatibleBuildType;
            return SessionManager<SessionPlayerData>.Instance.IsDuplicateConnection(payload.playerId)
                ? ConnectStatus.LoggedInAgain
                : ConnectStatus.Success;
        }
    }
}