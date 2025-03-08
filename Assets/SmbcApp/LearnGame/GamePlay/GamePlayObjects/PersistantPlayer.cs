using System;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.ConnectionManagement;
using SmbcApp.LearnGame.GamePlay.GamePlayObjects.Avatar;
using SmbcApp.LearnGame.GamePlay.GamePlayObjects.Character;
using SmbcApp.LearnGame.GamePlay.GamePlayObjects.RuntimeDataContainers;
using SmbcApp.LearnGame.Utils;
using Unity.Collections;
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
        [SerializeField] [Required] private PersistantPlayerRuntimeCollection runtimeCollection;
        [SerializeField] [Required] private NetworkAvatarGuidState networkAvatarGuidState;
        [SerializeField] [Required] private NetworkBalanceState networkBalanceState;
        [SerializeField] [Required] private NetworkStockState networkStockState;
        public NetworkVariable<FixedPlayerName> Name { get; } = new();
        public NetworkAvatarGuidState AvatarGuidState => networkAvatarGuidState;
        public NetworkBalanceState BalanceState => networkBalanceState;
        public NetworkStockState StockState => networkStockState;

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
            if (OwnerClientId == NetworkManager.ServerClientId)
            {
                Name.Value = "Server";
                return;
            }

            var sessionPlayerData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(OwnerClientId);
            if (!sessionPlayerData.HasValue) return;

            var playerData = sessionPlayerData.Value;
            Name.Value = playerData.PlayerName;

            if (playerData.HasCharacterSpawned)
            {
                networkAvatarGuidState.AvatarGuid.Value = playerData.AvatarNetworkGuid;
                networkBalanceState.CurrentBalance = playerData.CurrentBalance;
                networkStockState.SetStockAmount(playerData.StockAmounts);
            }
            else
            {
                networkAvatarGuidState.SetRandomAvatar();
                networkBalanceState.InitializeBalance();
                networkStockState.SetStockAmount(Array.Empty<int>());

                playerData.AvatarNetworkGuid = networkAvatarGuidState.AvatarGuid.Value;
                playerData.CurrentBalance = networkBalanceState.CurrentBalance;
                // 一旦空の配列を入れておいて、ゲーム開始時に初期化する
                playerData.StockAmounts = Array.Empty<int>();

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
            playerData.AvatarNetworkGuid = networkAvatarGuidState.AvatarGuid.Value;
            playerData.CurrentBalance = networkBalanceState.CurrentBalance;
            playerData.StockAmounts = networkStockState.ToArray();
            SessionManager<SessionPlayerData>.Instance.SetPlayerData(OwnerClientId, playerData);
        }
    }

    public struct FixedPlayerName : INetworkSerializable, IEquatable<FixedPlayerName>
    {
        private FixedString32Bytes _name;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _name);
        }

        public override string ToString()
        {
            return _name.Value;
        }

        public static implicit operator string(FixedPlayerName name)
        {
            return name.ToString();
        }

        public static implicit operator FixedPlayerName(string name)
        {
            return new FixedPlayerName { _name = new FixedString32Bytes(name) };
        }

        public bool Equals(FixedPlayerName other)
        {
            return _name.Equals(other._name);
        }

        public override bool Equals(object obj)
        {
            return obj is FixedPlayerName other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _name.GetHashCode();
        }
    }
}