using System;
using System.Text;
using Cysharp.Threading.Tasks;
using SmbcApp.LearnGame.GamePlay.Configuration;
using SmbcApp.LearnGame.UnityService.Session;
using Unity.Logging;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;
using UnityEngine;

namespace SmbcApp.LearnGame.ConnectionManagement
{
    internal abstract class ConnectionMethodBase
    {
        private readonly ConnectionManager _connectionManager;
        private readonly string _playerName;
        private readonly SessionServiceFacade _sessionServiceFacade;

        protected ConnectionMethodBase(
            ConnectionManager connectionManager,
            string playerName,
            SessionServiceFacade sessionServiceFacade
        )
        {
            _connectionManager = connectionManager;
            _playerName = playerName;
            _sessionServiceFacade = sessionServiceFacade;

            var networkManager = connectionManager.NetworkManager;
            if (networkManager.NetworkConfig.NetworkTransport is not UnityTransport transport) return;

#if UNITY_WEBGL
            transport.UseEncryption = true;
            transport.UseWebSockets = true;
#else
            transport.UseEncryption = GameConfiguration.Instance.ConnectionMethod ==
                                      GameConfiguration.ConnectionMethodType.Relay;
            transport.UseWebSockets = GameConfiguration.Instance.UseWebsockets;
#endif
        }

        public async UniTask SetupServerConnectionAsync()
        {
            Log.Info("Setting up server connection");
            if (!TryGetPlayerId(out var playerId))
                throw new ConnectionMethodException("Failed to get player id");

            // コネクションペイロードを設定
            SetConnectionPayload(playerId, _playerName);

            if (await _sessionServiceFacade.TryCreateSession(ServerSessionOptions))
                return;
            throw new ConnectionMethodException("Failed to create session");
        }

        public async UniTask SetupClientConnectionAsync(string sessionCode)
        {
            Log.Info("Setting up client connection");
            if (!TryGetPlayerId(out var playerId))
                throw new ConnectionMethodException("Failed to get player id");

            // コネクションペイロードを設定
            SetConnectionPayload(playerId, _playerName);

            if (!await _sessionServiceFacade.TryJoinSession(sessionCode))
                throw new ConnectionMethodException($"Failed to join session with code {sessionCode}");
            Log.Info("Successfully joined session with code {0}", sessionCode);
        }

        protected abstract SessionOptions ServerSessionOptions(SessionOptions opts);

        private void SetConnectionPayload(string playerId, string playerName)
        {
            var payload = JsonUtility.ToJson(new ConnectionPayload
            {
                playerId = playerId,
                playerName = playerName,
                isDebug = Debug.isDebugBuild
            });
            var bytes = Encoding.UTF8.GetBytes(payload);

            _connectionManager.NetworkManager.NetworkConfig.ConnectionData = bytes;
        }

        private static bool TryGetPlayerId(out string playerId)
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
                GameConfiguration.ConnectionMethodType.Direct => new ConnectionMethodDirect(
                    connectionManager,
                    playerName,
                    sessionServiceFacade
                ),
                GameConfiguration.ConnectionMethodType.Relay => new ConnectionMethodRelay(
                    connectionManager,
                    playerName,
                    sessionServiceFacade
                ),
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

    internal sealed class ConnectionMethodDirect : ConnectionMethodBase
    {
        public ConnectionMethodDirect(
            ConnectionManager connectionManager,
            string playerName,
            SessionServiceFacade sessionServiceFacade
        ) : base(connectionManager, playerName, sessionServiceFacade)
        {
        }

        protected override SessionOptions ServerSessionOptions(SessionOptions opts)
        {
            return opts.WithDirectNetwork();
        }
    }

    internal sealed class ConnectionMethodRelay : ConnectionMethodBase
    {
        public ConnectionMethodRelay(
            ConnectionManager connectionManager,
            string playerName,
            SessionServiceFacade sessionServiceFacade
        ) : base(connectionManager, playerName, sessionServiceFacade)
        {
        }

        protected override SessionOptions ServerSessionOptions(SessionOptions opts)
        {
            return opts.WithRelayNetwork();
        }
    }
}