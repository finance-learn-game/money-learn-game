using System.Text;
using Cysharp.Threading.Tasks;
using SmbcApp.LearnGame.UnityService.Session;
using Unity.Logging;
using Unity.Services.Authentication;
using UnityEngine;

namespace SmbcApp.LearnGame.ConnectionManagement
{
    internal abstract class ConnectionMethodBase
    {
        private readonly ConnectionManager _connectionManager;
        protected readonly string PlayerName;

        protected ConnectionMethodBase(ConnectionManager connectionManager, string playerName)
        {
            _connectionManager = connectionManager;
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

            _connectionManager.NetworkManager.NetworkConfig.ConnectionData = bytes;
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
    }

    internal sealed class ConnectionMethodMultiPlaySDK : ConnectionMethodBase
    {
        private readonly SessionServiceFacade _sessionServiceFacade;

        public ConnectionMethodMultiPlaySDK(SessionServiceFacade sessionServiceFacade,
            ConnectionManager connectionManager, string playerName) : base(connectionManager, playerName)
        {
            _sessionServiceFacade = sessionServiceFacade;
        }

        public override UniTask SetupServerConnectionAsync()
        {
            Log.Info("Setting up MultiPlayer SDK server connection");
            if (!TryGetPlayerId(out var playerId))
            {
                Log.Error("Failed to get player id");
                return UniTask.CompletedTask;
            }

            SetConnectionPayload(playerId, PlayerName);
            return UniTask.CompletedTask;
        }

        public override UniTask SetupClientConnectionAsync()
        {
            Log.Info("Setting up MultiPlayer SDK client connection");
            if (!TryGetPlayerId(out var playerId))
            {
                Log.Error("Failed to get player id");
                return UniTask.CompletedTask;
            }

            SetConnectionPayload(playerId, PlayerName);

            if (_sessionServiceFacade.CurrentSession != null) return UniTask.CompletedTask;
            Log.Warning("No session to connect to");
            return UniTask.CompletedTask;
        }
    }
}