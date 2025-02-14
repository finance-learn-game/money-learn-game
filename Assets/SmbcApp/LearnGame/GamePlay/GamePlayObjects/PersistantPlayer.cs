using Sirenix.OdinInspector;
using SmbcApp.LearnGame.ConnectionManagement;
using SmbcApp.LearnGame.GamePlay.GamePlayObjects.Avatar;
using SmbcApp.LearnGame.GamePlay.GamePlayObjects.RuntimeDataContainers;
using SmbcApp.LearnGame.Utils;
using Unity.Netcode;
using UnityEngine;

namespace SmbcApp.LearnGame.GamePlay.GamePlayObjects
{
    /// <summary>
    ///     NetworkManagerのデフォルトプレハブとして登録されるプレイヤーオブジェクト。
    /// </summary>
    [RequireComponent(typeof(NetworkObject))]
    internal sealed class PersistantPlayer : NetworkBehaviour
    {
        [SerializeField] private PersistantPlayerRuntimeCollection runtimeCollection;
        [SerializeField] private NetworkAvatarGuidState networkAvatarGuidState;
        [ReadOnly] public NetworkVariable<string> Name { get; } = new();

        public override void OnDestroy()
        {
            base.OnDestroy();
            RemovePersistantPlayer();
        }

        public override void OnNetworkSpawn()
        {
            gameObject.name = "PersistentPlayer " + OwnerClientId;

            runtimeCollection.AddPlayer(this);
            if (!IsServer) return;

            var sessionPlayerData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(OwnerClientId);
            if (!sessionPlayerData.HasValue) return;

            var playerData = sessionPlayerData.Value;
            Name.Value = playerData.PlayerName;
            if (playerData.HasCharacterSpawned)
            {
                networkAvatarGuidState.avatarGuid.Value = playerData.AvatarNetworkGuid;
            }
            else
            {
                networkAvatarGuidState.SetRandomAvatar();
                playerData.AvatarNetworkGuid = networkAvatarGuidState.avatarGuid.Value;
                SessionManager<SessionPlayerData>.Instance.SetPlayerData(OwnerClientId, playerData);
            }
        }

        public override void OnNetworkDespawn()
        {
            RemovePersistantPlayer();
        }

        private void RemovePersistantPlayer()
        {
            runtimeCollection.RemovePlayer(this);
            if (!IsServer) return;

            var sessionPlayerData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(OwnerClientId);
            if (!sessionPlayerData.HasValue) return;

            var playerData = sessionPlayerData.Value;
            playerData.PlayerName = Name.Value;
            playerData.AvatarNetworkGuid = networkAvatarGuidState.avatarGuid.Value;
            SessionManager<SessionPlayerData>.Instance.SetPlayerData(OwnerClientId, playerData);
        }
    }
}