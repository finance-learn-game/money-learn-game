using System;
using Unity.Collections;
using Unity.Netcode;

namespace SmbcApp.LearnGame.GamePlay.GameState.NetworkData
{
    /// <summary>
    ///     プレイヤーごとの保有している株式情報を管理する
    /// </summary>
    internal sealed class NetworkStockData : NetworkBehaviour
    {
        private NetworkList<PlayerStockData> _playerStocks;

        private int ClientId2Index(ulong clientId)
        {
            for (var i = 0; i < _playerStocks.Count; i++)
                if (_playerStocks[i].ClientId == clientId)
                    return i;
            return -1;
        }

        public bool TryGetPlayerStock(ulong clientId, out ReadOnlySpan<int> stockAmounts)
        {
            var index = ClientId2Index(clientId);
            if (index == -1)
            {
                stockAmounts = default;
                return false;
            }

            stockAmounts = _playerStocks[index].StockAmounts;
            return true;
        }

        public void SetPlayerStock(ulong clientId, in NativeArray<int> stockAmounts)
        {
            var index = ClientId2Index(clientId);
            if (index == -1)
            {
                _playerStocks.Add(new PlayerStockData(clientId, stockAmounts));
            }
            else
            {
                _playerStocks[index].Dispose();
                _playerStocks[index] = new PlayerStockData(clientId, stockAmounts);
            }
        }

        public override void OnNetworkDespawn()
        {
            foreach (var playerStockData in _playerStocks) playerStockData.Dispose();
        }

        /// <summary>
        ///     プレイヤーごとの保有している株式情報を保存する
        /// </summary>
        private struct PlayerStockData : INetworkSerializable, IEquatable<PlayerStockData>, IDisposable
        {
            private ulong _clientId;
            private NativeArray<int> _stockAmounts;

            public ulong ClientId => _clientId;
            public ReadOnlySpan<int> StockAmounts => _stockAmounts;

            public PlayerStockData(ulong clientId, NativeArray<int> stockAmounts)
            {
                _clientId = clientId;
                _stockAmounts = stockAmounts;
            }

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref _clientId);
                serializer.SerializeValue(ref _stockAmounts, Allocator.Persistent);
            }

            public bool Equals(PlayerStockData other)
            {
                return _clientId == other._clientId && _stockAmounts.Equals(other._stockAmounts);
            }

            public override bool Equals(object obj)
            {
                return obj is PlayerStockData other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(_clientId, _stockAmounts);
            }

            public void Dispose()
            {
                _stockAmounts.Dispose();
            }
        }
    }
}