using R3;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.Utils;
using Unity.Netcode;
using UnityEngine;

namespace SmbcApp.LearnGame.Gameplay.GameState
{
    [RequireComponent(typeof(NetCodeHooks))]
    internal abstract class ServerGameStateBehaviour : GameStateBehaviour
    {
        [SerializeField] [Required] protected NetCodeHooks netCodeHooks;

        protected override void Awake()
        {
            base.Awake();

            netCodeHooks.OnNetworkSpawnHook.Subscribe(OnNetworkSpawn)
                .AddTo(gameObject);
            netCodeHooks.OnNetworkDespawnHook.Subscribe(OnNetworkDespawn)
                .AddTo(gameObject);
        }

        private void OnNetworkSpawn(Unit _)
        {
            var networkManager = NetworkManager.Singleton;
            if (!networkManager.IsServer)
            {
                enabled = false;
            }
            else
            {
                networkManager.OnClientDisconnectCallback += OnClientDisconnect;
                networkManager.SceneManager.OnSceneEvent += OnSceneEvent;
                OnServerNetworkSpawn();
            }
        }

        protected virtual void OnServerNetworkSpawn()
        {
        }

        protected virtual void OnServerNetworkDespawn()
        {
        }

        private void OnNetworkDespawn(Unit _)
        {
            var networkManager = NetworkManager.Singleton;
            if (!networkManager) return;

            networkManager.OnClientDisconnectCallback -= OnClientDisconnect;
            networkManager.SceneManager.OnSceneEvent -= OnSceneEvent;
            OnServerNetworkDespawn();
        }

        protected abstract void OnClientDisconnect(ulong clientId);
        protected abstract void OnSceneEvent(SceneEvent sceneEvent);
    }
}