using System.Collections.Generic;
using System.Linq;
using Unity.Logging;

namespace SmbcApp.LearnGame.Utils
{
    public interface ISessionPlayerData
    {
        bool IsConnected { get; set; }
        ulong ClientID { get; set; }
        void Reinitialize();
    }

    /// <summary>
    ///     このクラスは、プレイヤーをセッションにバインドするために一意のプレイヤーIDを使用します。
    ///     プレイヤーがホストに接続すると、ホストは現在のClientIDをプレイヤーの一意のIDに関連付けます。
    ///     プレイヤーが切断して同じホストに再接続すると、セッションが保持されます。
    /// </summary>
    /// <remarks>
    ///     クライアントが生成したプレイヤーIDを直接送信すると、悪意のあるユーザーがそれを傍受して元のユーザーをなりすますことができる可能性があるため、
    ///     問題が発生する可能性があります。
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public class SessionManager<T> where T : struct, ISessionPlayerData
    {
        private static SessionManager<T> _instance;

        /// <summary>
        ///     Maps a given client player id to the data for a given client player.
        /// </summary>
        private readonly Dictionary<string, T> _clientData;

        /// <summary>
        ///     Map to allow us to cheaply map from player id to player data.
        /// </summary>
        private readonly Dictionary<ulong, string> _clientIDToPlayerId;

        private bool _hasSessionStarted;

        private SessionManager()
        {
            _clientData = new Dictionary<string, T>();
            _clientIDToPlayerId = new Dictionary<ulong, string>();
        }

        public static SessionManager<T> Instance => _instance ??= new SessionManager<T>();

        /// <summary>
        ///     Handles client disconnect.
        /// </summary>
        public void DisconnectClient(ulong clientId)
        {
            if (_hasSessionStarted)
            {
                // Mark client as disconnected, but keep their data so they can reconnect.
                if (!_clientIDToPlayerId.TryGetValue(clientId, out var playerId)) return;
                var playerData = GetPlayerData(playerId);
                if (playerData == null || playerData.Value.ClientID != clientId) return;
                var clientData = _clientData[playerId];
                clientData.IsConnected = false;
                _clientData[playerId] = clientData;
            }
            else
            {
                // Session has not started, no need to keep their data
                if (!_clientIDToPlayerId.Remove(clientId, out var playerId)) return;
                var playerData = GetPlayerData(playerId);
                if (playerData != null && playerData.Value.ClientID == clientId) _clientData.Remove(playerId);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="playerId">
        ///     This is the playerId that is unique to this client and persists across multiple logins from the
        ///     same client
        /// </param>
        /// <returns>True if a player with this ID is already connected.</returns>
        public bool IsDuplicateConnection(string playerId)
        {
            return _clientData.ContainsKey(playerId) && _clientData[playerId].IsConnected;
        }

        /// <summary>
        ///     Adds a connecting player's session data if it is a new connection, or updates their session data in case of a
        ///     reconnection.
        /// </summary>
        /// <param name="clientId">
        ///     This is the clientId that Netcode assigned us on login. It does not persist across multiple
        ///     logins from the same client.
        /// </param>
        /// <param name="playerId">
        ///     This is the playerId that is unique to this client and persists across multiple logins from the
        ///     same client
        /// </param>
        /// <param name="sessionPlayerData">The player's initial data</param>
        public void SetupConnectingPlayerSessionData(ulong clientId, string playerId, T sessionPlayerData)
        {
            var isReconnecting = false;

            // Test for duplicate connection
            if (IsDuplicateConnection(playerId))
            {
                Log.Error(
                    "Player ID {0} already exists. This is a duplicate connection. Rejecting this session data.",
                    playerId);
                return;
            }

            // If another client exists with the same playerId
            if (_clientData.TryGetValue(playerId, out var value))
                if (!value.IsConnected)
                    // If this connecting client has the same player Id as a disconnected client, this is a reconnection.
                    isReconnecting = true;

            // Reconnecting. Give data from old player to new player
            if (isReconnecting)
            {
                // Update player session data
                sessionPlayerData = _clientData[playerId];
                sessionPlayerData.ClientID = clientId;
                sessionPlayerData.IsConnected = true;
            }

            //Populate our dictionaries with the SessionPlayerData
            _clientIDToPlayerId[clientId] = playerId;
            _clientData[playerId] = sessionPlayerData;
        }

        /// <summary>
        /// </summary>
        /// <param name="clientId"> id of the client whose data is requested</param>
        /// <returns>The Player ID matching the given client ID</returns>
        public string GetPlayerId(ulong clientId)
        {
            if (_clientIDToPlayerId.TryGetValue(clientId, out var playerId)) return playerId;

            Log.Info("No client player ID found mapped to the given client ID: {0}", clientId);
            return null;
        }

        /// <summary>
        /// </summary>
        /// <param name="clientId"> id of the client whose data is requested</param>
        /// <returns>Player data struct matching the given ID</returns>
        public T? GetPlayerData(ulong clientId)
        {
            //First see if we have a playerId matching the clientID given.
            var playerId = GetPlayerId(clientId);
            if (playerId != null) return GetPlayerData(playerId);

            Log.Info("No client player ID found mapped to the given client ID: {0}", clientId);
            return null;
        }

        /// <summary>
        /// </summary>
        /// <param name="playerId"> Player ID of the client whose data is requested</param>
        /// <returns>Player data struct matching the given ID</returns>
        public T? GetPlayerData(string playerId)
        {
            if (_clientData.TryGetValue(playerId, out var data)) return data;

            Log.Info("No PlayerData of matching player ID found: {0}", playerId);
            return null;
        }

        /// <summary>
        ///     Updates player data
        /// </summary>
        /// <param name="clientId"> id of the client whose data will be updated </param>
        /// <param name="sessionPlayerData"> new data to overwrite the old </param>
        public void SetPlayerData(ulong clientId, T sessionPlayerData)
        {
            if (_clientIDToPlayerId.TryGetValue(clientId, out var playerId))
                _clientData[playerId] = sessionPlayerData;
            else
                Log.Error("No client player ID found mapped to the given client ID: {0}", clientId);
        }

        /// <summary>
        ///     Marks the current session as started, so from now on we keep the data of disconnected players.
        /// </summary>
        public void OnSessionStarted()
        {
            _hasSessionStarted = true;
        }

        /// <summary>
        ///     Reinitializes session data from connected players, and clears data from disconnected players, so that if they
        ///     reconnect in the next game, they will be treated as new players
        /// </summary>
        public void OnSessionEnded()
        {
            ClearDisconnectedPlayersData();
            ReinitializePlayersData();
            _hasSessionStarted = false;
        }

        /// <summary>
        ///     Resets all our runtime state, so it is ready to be reinitialized when starting a new server
        /// </summary>
        public void OnServerEnded()
        {
            _clientData.Clear();
            _clientIDToPlayerId.Clear();
            _hasSessionStarted = false;
        }

        private void ReinitializePlayersData()
        {
            foreach (var id in _clientIDToPlayerId.Keys)
            {
                var playerId = _clientIDToPlayerId[id];
                var sessionPlayerData = _clientData[playerId];
                sessionPlayerData.Reinitialize();
                _clientData[playerId] = sessionPlayerData;
            }
        }

        private void ClearDisconnectedPlayersData()
        {
            var idsToClear = (from id in _clientIDToPlayerId.Keys
                let data = GetPlayerData(id)
                where data is { IsConnected: false }
                select id).ToList();

            foreach (var id in idsToClear)
            {
                var playerId = _clientIDToPlayerId[id];
                var playerData = GetPlayerData(playerId);
                if (playerData != null && playerData.Value.ClientID == id) _clientData.Remove(playerId);

                _clientIDToPlayerId.Remove(id);
            }
        }
    }
}