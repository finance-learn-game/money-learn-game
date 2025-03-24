using System;
using R3;
using SmbcApp.LearnGame.ConnectionManagement;
using SmbcApp.LearnGame.GamePlay.Configuration;
using SmbcApp.LearnGame.GamePlay.GamePlayObjects;
using SmbcApp.LearnGame.GamePlay.GameState.NetworkData;
using SmbcApp.LearnGame.Infrastructure;
using SmbcApp.LearnGame.SceneLoader;
using SmbcApp.LearnGame.Utils;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace SmbcApp.LearnGame.Gameplay.GameState
{
    [RequireComponent(typeof(NetworkAvatarSelection))]
    internal sealed class ServerAvatarSelectState : ServerGameStateBehaviour
    {
        private NetworkAvatarSelection _networkAvatarSelection;

        [Inject] internal ConnectionManager ConnectionManager;
        [Inject] internal SceneLoader.SceneLoader SceneLoader;

        public override GameState ActiveState => GameState.AvatarSelect;

        protected override void Awake()
        {
            base.Awake();
            TryGetComponent(out _networkAvatarSelection);
        }

        private int FindSessionPlayerIdx(ulong clientId)
        {
            for (var i = 0; i < _networkAvatarSelection.SessionPlayers.Count; i++)
                if (_networkAvatarSelection.SessionPlayers[i].ClientId == clientId)
                    return i;
            return -1;
        }

        /// <summary>
        ///     全員が選択完了しているか確認し、IsAvatarSelectFinishedを更新する
        /// </summary>
        private void CheckIfReady()
        {
            foreach (var playerInfo in _networkAvatarSelection.SessionPlayers)
            {
                // サーバーは除外
                if (playerInfo.ClientId == NetworkManager.ServerClientId) continue;
                if (playerInfo.IsReady) continue;

                _networkAvatarSelection.IsAvatarSelectFinished.Value = false;
                return;
            }

            _networkAvatarSelection.IsAvatarSelectFinished.Value = true;
        }

        private bool IsPlayerNumberAvailable(int playerNumber)
        {
            var found = false;
            foreach (var playerInfo in _networkAvatarSelection.SessionPlayers)
                if (playerInfo.PlayerNumber == playerNumber)
                {
                    found = true;
                    break;
                }

            return !found;
        }

        private int GetAvailablePlayerNumber()
        {
            for (var i = 0; i < GameConfiguration.Instance.MaxPlayers; i++)
                if (IsPlayerNumberAvailable(i))
                    return i;

            return -1;
        }

        private void AddNewPlayer(ulong clientId)
        {
            var data = SessionManager<SessionPlayerData>.Instance.GetPlayerData(clientId);
            if (!data.HasValue) return;

            var playerData = data.Value;
            if (playerData.PlayerNumber == -1 || !IsPlayerNumberAvailable(playerData.PlayerNumber))
                playerData.PlayerNumber = GetAvailablePlayerNumber();

            if (playerData.PlayerNumber == -1)
                throw new InvalidOperationException("Player number is not available");

            _networkAvatarSelection.SessionPlayers.Add(
                new NetworkAvatarSelection.SessionPlayerState(
                    clientId, playerData.PlayerName, false, playerData.AvatarNetworkGuid, playerData.PlayerNumber
                )
            );
            SessionManager<SessionPlayerData>.Instance.SetPlayerData(clientId, playerData);
        }

        private void SaveAvatarSelectResult()
        {
            var networkManager = NetworkManager.Singleton;
            foreach (var playerInfo in _networkAvatarSelection.SessionPlayers)
            {
                var playerNetworkObj = networkManager.SpawnManager.GetPlayerNetworkObject(playerInfo.ClientId);
                if (playerNetworkObj && playerNetworkObj.TryGetComponent(out PersistantPlayer persistantPlayer))
                    persistantPlayer.AvatarGuidState.AvatarGuid.Value = playerInfo.AvatarGuid;
            }
        }

        private void OnStartGame(Unit _)
        {
            SaveAvatarSelectResult();
            SceneLoader.LoadScene(AppScenes.GameScene, true);
        }

        #region Callbacks

        /// <summary>
        ///     クライアントの状態変化時のコールバック
        /// </summary>
        /// <remarks>SessionPlayersの値を詰め替えて更新を通知</remarks>
        /// <param name="state">変化後の状態</param>
        /// <exception cref="InvalidOperationException">プレイヤー番号が見つからないとき</exception>
        private void OnClientChangeState(NetworkAvatarSelection.ChangeStateParams state)
        {
            if (state.ClientId == NetworkManager.ServerClientId) return;

            var idx = FindSessionPlayerIdx(state.ClientId);
            if (idx == -1) throw new InvalidOperationException("Client not found in session players");
            if (state.Avatar != null && state.Avatar.Value.ToGuid() == Guid.Empty)
                state = state with { IsReady = false };

            // 更新して通知
            var prev = _networkAvatarSelection.SessionPlayers[idx];
            prev.AvatarGuid = state.Avatar ?? prev.AvatarGuid;
            prev.IsReady = state.IsReady ?? prev.IsReady;
            _networkAvatarSelection.SessionPlayers[idx] = prev;

            CheckIfReady();
        }

        protected override void OnServerNetworkSpawn()
        {
            _networkAvatarSelection.OnStateChange.Subscribe(OnClientChangeState).AddTo(gameObject);
            _networkAvatarSelection.OnStartGame.Subscribe(OnStartGame).AddTo(gameObject);
        }

        protected override void OnClientDisconnect(ulong clientId)
        {
            for (var i = 0; i < _networkAvatarSelection.SessionPlayers.Count; i++)
                if (_networkAvatarSelection.SessionPlayers[i].ClientId == clientId)
                {
                    _networkAvatarSelection.SessionPlayers.RemoveAt(i);
                    break;
                }

            // 抜けたことにより、全員が選択完了しているか確認
            if (!_networkAvatarSelection.IsAvatarSelectFinished.Value)
                CheckIfReady();
        }

        protected override void OnSceneEvent(SceneEvent sceneEvent)
        {
            // シーンがロード完了したら、新規プレイヤーを追加
            if (sceneEvent.SceneEventType != SceneEventType.LoadComplete) return;
            AddNewPlayer(sceneEvent.ClientId);
        }

        #endregion
    }
}