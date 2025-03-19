using System;
using System.Text;
using Cysharp.Threading.Tasks;
using SmbcApp.LearnGame.GamePlay.Configuration;
using SmbcApp.LearnGame.UnityService.Session;
using Unity.Logging;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace SmbcApp.LearnGame.ConnectionManagement
{
    internal abstract class ConnectionMethodBase
    {
        protected readonly ConnectionManager ConnectionManager;
        protected readonly string PlayerName;

        protected ConnectionMethodBase(ConnectionManager connectionManager, string playerName)
        {
            ConnectionManager = connectionManager;
            PlayerName = playerName;
        }

        public abstract UniTask SetupServerConnectionAsync();
        public abstract UniTask SetupClientConnectionAsync();

        protected void SetConnectionPayload(string playerId, string playerName)
        {
            var payload = JsonUtility.ToJson(new ConnectionPayload
            {
                playerId = playerId,
                playerName = playerName,
                isDebug = Debug.isDebugBuild
            });
            var bytes = Encoding.UTF8.GetBytes(payload);

            ConnectionManager.NetworkManager.NetworkConfig.ConnectionData = bytes;
        }

        protected static bool TryGetPlayerId(out string playerId)
        {
            var isSignedIn = AuthenticationService.Instance.IsSignedIn;
            if (isSignedIn)
            {
                playerId = AuthenticationService.Instance.PlayerId;
                return true;
            }

            playerId = null;
            return false;
        }

        /// <summary>
        ///     現在のゲーム設定に基づいて、適切な接続方法を作成します。
        /// </summary>
        public static ConnectionMethodBase CreateConnectionMethod(
            SessionServiceFacade sessionServiceFacade,
            ConnectionManager connectionManager,
            string playerName
        )
        {
            return GameConfiguration.Instance.ConnectionMethod switch
            {
                GameConfiguration.ConnectionMethodType.IP => new ConnectionMethodIP(sessionServiceFacade,
                    connectionManager, playerName),
                GameConfiguration.ConnectionMethodType.Relay => new ConnectionMethodRelay(sessionServiceFacade,
                    connectionManager, playerName),
                _ => throw new ConnectionMethodException("Unknown connection type")
            };
        }
    }

    public sealed class ConnectionMethodException : Exception
    {
        public ConnectionMethodException(string message) : base(message)
        {
        }
    }

    internal sealed class ConnectionMethodIP : ConnectionMethodBase
    {
        private readonly SessionServiceFacade _sessionServiceFacade;

        public ConnectionMethodIP(
            SessionServiceFacade sessionServiceFacade,
            ConnectionManager connectionManager,
            string playerName
        ) : base(connectionManager, playerName)
        {
            _sessionServiceFacade = sessionServiceFacade;
        }

        public override UniTask SetupServerConnectionAsync()
        {
            Log.Info("Setting up MultiPlayer SDK server connection");
            if (!TryGetPlayerId(out var playerId))
                throw new ConnectionMethodException("Failed to get player id");

            SetConnectionPayload(playerId, PlayerName);
            return UniTask.CompletedTask;
        }

        public override UniTask SetupClientConnectionAsync()
        {
            Log.Info("Setting up Unity Relay client");
            if (!TryGetPlayerId(out var playerId))
                throw new ConnectionMethodException("Failed to get player id");

            // コネクションペイロードを設定
            SetConnectionPayload(playerId, PlayerName);

            if (_sessionServiceFacade.CurrentSession != null) return UniTask.CompletedTask;
            throw new ConnectionMethodException("No session to connect to");
        }
    }

    internal sealed class ConnectionMethodRelay : ConnectionMethodBase
    {
        private readonly SessionServiceFacade _sessionServiceFacade;

        public ConnectionMethodRelay(
            SessionServiceFacade sessionServiceFacade,
            ConnectionManager connectionManager,
            string playerName
        ) : base(connectionManager, playerName)
        {
            _sessionServiceFacade = sessionServiceFacade;
        }

        public override async UniTask SetupServerConnectionAsync()
        {
            Log.Info("Setting up MultiPlayer SDK server connection");
            if (!TryGetPlayerId(out var playerId))
                throw new ConnectionMethodException("Failed to get player id");

            SetConnectionPayload(playerId, PlayerName);

            // 割り当てを作成
            var maxPlayer = GameConfiguration.Instance.MaxPlayers;
            var hostAllocation = await RelayService.Instance.CreateAllocationAsync(maxPlayer);
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(hostAllocation.AllocationId);

            Log.Info(
                "HostAllocation: server: connection data: {0} {1}, allocation id: {2}, region: {3}",
                hostAllocation.ConnectionData[0], hostAllocation.ConnectionData[1],
                hostAllocation.AllocationId,
                hostAllocation.Region
            );

            await _sessionServiceFacade.SetSessionProperty(
                SessionServiceFacade.RelayJoinCodeKey,
                new SessionProperty(joinCode)
            );

            // UnityTransportの接続データを設定
            var utp = (UnityTransport)ConnectionManager.NetworkManager.NetworkConfig.NetworkTransport;
            var connectionType = GameConfiguration.Instance.ConnectionType;
            utp.SetRelayServerData(hostAllocation.ToRelayServerData(connectionType));

            Log.Info("Created Unity Relay server with join code {0}", joinCode);
        }

        public override async UniTask SetupClientConnectionAsync()
        {
            Log.Info("Setting up Unity Relay client");
            if (!TryGetPlayerId(out var playerId))
                throw new ConnectionMethodException("Failed to get player id");

            // コネクションペイロードを設定
            SetConnectionPayload(playerId, PlayerName);

            // RelayJoinCodeをセッションプロパティから取得
            if (!_sessionServiceFacade.TryGetSessionProperty(
                    SessionServiceFacade.RelayJoinCodeKey,
                    out var relayJoinCode
                ))
                throw new ConnectionMethodException("No relay join code found in session properties");

            Log.Info("Setting Unity Relay client with join code {0}", relayJoinCode);

            // クライアント参加割り当てを参加コードから作成
            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode.Value);
            Log.Info(
                "JoinAllocation: client -> {0} {1} ({2}), server -> {3} {4}",
                joinAllocation.ConnectionData[0], joinAllocation.ConnectionData[1],
                joinAllocation.AllocationId,
                joinAllocation.HostConnectionData[0], joinAllocation.HostConnectionData[1]
            );

            // UnityTransportの接続データを設定
            var utp = (UnityTransport)ConnectionManager.NetworkManager.NetworkConfig.NetworkTransport;
            var connectionType = GameConfiguration.Instance.ConnectionType;
            utp.SetRelayServerData(joinAllocation.ToRelayServerData(connectionType));
        }
    }
}