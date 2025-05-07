using System;
using SmbcApp.LearnGame.Infrastructure;
using SmbcApp.LearnGame.Utils;
using Unity.Netcode;
using UnityEngine;

namespace SmbcApp.LearnGame.ConnectionManagement
{
    public struct SessionPlayerData : ISessionPlayerData
    {
        public string PlayerName;
        public int PlayerNumber;
        public NetworkGuid AvatarNetworkGuid;
        public int CurrentBalance;
        public bool HasCharacterSpawned;

        public SessionPlayerData(
            ulong clientID,
            string playerName,
            in NetworkGuid avatarNetworkGuid,
            int currentBalance,
            int[] stockAmounts,
            TownPartData[] townPartsAmounts,
            bool isConnected = false,
            bool hasCharacterSpawned = false
        )
        {
            PlayerName = playerName;
            PlayerNumber = -1;
            AvatarNetworkGuid = avatarNetworkGuid;
            CurrentBalance = currentBalance;
            HasCharacterSpawned = hasCharacterSpawned;
            IsConnected = isConnected;
            ClientID = clientID;
            StockAmounts = stockAmounts;
            TownPartsAmounts = townPartsAmounts;
        }

        public bool IsConnected { get; set; }
        public ulong ClientID { get; set; }
        public int[] StockAmounts { get; set; }
        public TownPartData[] TownPartsAmounts { get; set; }

        public void Reinitialize()
        {
            HasCharacterSpawned = false;
        }
    }

    public struct TownPartData : INetworkSerializable, IEquatable<TownPartData>
    {
        // PartsRegistry上でのパーツのインデックスをIDとして扱う
        private int _partId;
        private Vector3 _position;
        private bool _isPlaced;
        private NetworkGuid _dataId;

        public TownPartData(int partId, Vector3 position, bool isPlaced)
        {
            _partId = partId;
            _position = position;
            _isPlaced = isPlaced;
            _dataId = Guid.NewGuid().ToNetworkGuid();
        }

        public int PartId => _partId;
        public Vector3 Position => _position;
        public bool IsPlaced => _isPlaced;
        public NetworkGuid DataId => _dataId;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _partId);
            serializer.SerializeValue(ref _position);
            serializer.SerializeValue(ref _isPlaced);
            serializer.SerializeValue(ref _dataId);
        }

        public TownPartData CopyWith(in Vector3 position, bool isPlaced)
        {
            return new TownPartData(PartId, position, isPlaced)
            {
                _dataId = _dataId
            };
        }

        public bool Equals(TownPartData other)
        {
            return _partId == other._partId &&
                   _position.Equals(other._position) &&
                   _isPlaced == other._isPlaced &&
                   _dataId.Equals(other._dataId);
        }

        public override bool Equals(object obj)
        {
            return obj is TownPartData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_partId, _position, _isPlaced, _dataId);
        }
    }
}