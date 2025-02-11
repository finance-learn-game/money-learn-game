using System.Collections.Generic;
using R3;
using Unity.Netcode;
using UnityEngine;

namespace SmbcApp.LearnGame.Utils
{
    /// <summary>
    ///     ローカルインスタンスとリモートインスタンスのシーン読み込み進行状況に関するデータを管理
    /// </summary>
    public sealed class LoadingProgressManager : NetworkBehaviour
    {
        [SerializeField] private NetworkedLoadingProgressTracker progressTrackerPrefab;

        private readonly Subject<Unit> _onTrackersUpdated = new();
        private bool _isLoading;
        private AsyncOperation _localLoadOperation;
        private float _localProgress;

        public Observable<Unit> OnTrackersUpdated => _onTrackersUpdated;

        /// <summary>
        ///     各クライアント毎のロード進行状況への参照を保持、クライアントIDをキーとする
        /// </summary>
        public Dictionary<ulong, NetworkedLoadingProgressTracker> ProgressTrackers { get; } = new();

        public AsyncOperation LocalLoadOperation
        {
            set
            {
                _isLoading = true;
                _localLoadOperation = value;
                _localProgress = 0f;
            }
        }

        public float LocalProgress
        {
            get => IsSpawned && ProgressTrackers.ContainsKey(NetworkManager.LocalClientId)
                ? ProgressTrackers[NetworkManager.LocalClientId].Progress.Value
                : _localProgress;
            private set
            {
                if (IsSpawned && ProgressTrackers.ContainsKey(NetworkManager.LocalClientId))
                    ProgressTrackers[NetworkManager.LocalClientId].Progress.Value = value;
                else
                    _localProgress = value;
            }
        }

        private void Update()
        {
            if (_localLoadOperation == null || !_isLoading) return;

            if (_localLoadOperation.isDone)
            {
                _isLoading = false;
                _localProgress = 1f;
            }
            else
            {
                _localProgress = _localLoadOperation.progress;
            }
        }

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;

            NetworkManager.OnClientConnectedCallback += AddTracker;
            NetworkManager.OnClientDisconnectCallback += RemoveTracker;
            AddTracker(NetworkManager.LocalClientId);
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer)
            {
                NetworkManager.OnClientConnectedCallback -= AddTracker;
                NetworkManager.OnClientDisconnectCallback -= RemoveTracker;
            }

            ProgressTrackers.Clear();
            _onTrackersUpdated.OnNext(Unit.Default);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void ClientUpdateTrackersRpc()
        {
            if (!IsHost)
            {
                ProgressTrackers.Clear();
                foreach (var tracker in FindObjectsByType<NetworkedLoadingProgressTracker>(FindObjectsSortMode.None))
                {
                    // トラッカーがデスポーンされているが、まだ破棄されていない場合は追加しない
                    if (!tracker.IsSpawned) continue;

                    ProgressTrackers[tracker.OwnerClientId] = tracker;
                    if (tracker.OwnerClientId == NetworkManager.LocalClientId)
                        LocalProgress = Mathf.Max(_localProgress, LocalProgress);
                }
            }

            _onTrackersUpdated.OnNext(Unit.Default);
        }

        private void AddTracker(ulong clientId)
        {
            if (!IsServer) return;
            if (ProgressTrackers.ContainsKey(clientId)) return;

            var tracker = Instantiate(progressTrackerPrefab);
            tracker.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
            ProgressTrackers.Add(clientId, tracker);
            ClientUpdateTrackersRpc();
        }

        private void RemoveTracker(ulong clientId)
        {
            if (!IsServer) return;
            if (!ProgressTrackers.TryGetValue(clientId, out var tracker)) return;

            tracker.NetworkObject.Despawn();
            ProgressTrackers.Remove(clientId);
            ClientUpdateTrackersRpc();
        }
    }
}