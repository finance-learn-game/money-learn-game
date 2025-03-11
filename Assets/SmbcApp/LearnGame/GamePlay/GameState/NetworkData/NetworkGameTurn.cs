using System;
using R3;
using SmbcApp.LearnGame.Utils;
using Unity.Logging;
using Unity.Netcode;

namespace SmbcApp.LearnGame.GamePlay.GameState.NetworkData
{
    /// <summary>
    ///     インゲームでのターンを管理する
    /// </summary>
    internal sealed class NetworkGameTurn : NetworkBehaviour
    {
        private readonly NetworkVariable<long> _currentTimeTick = new();
        private readonly Subject<Unit> _onTurnEnd = new();
        private NetworkList<ClientTurn> _isTurnEndList;

        public Observable<Unit> OnTurnEnd => _onTurnEnd;

        public DateTime CurrentTime
        {
            get => new(_currentTimeTick.Value);
            set
            {
                if (IsServer) _currentTimeTick.Value = value.Ticks;
            }
        }

        public Observable<DateTime> OnChangeTime =>
            _currentTimeTick.OnChangedAsObservable().Select(t => new DateTime(t));

        private void Awake()
        {
            _isTurnEndList = new NetworkList<ClientTurn>();
        }

        public Observable<bool> ObserveChangeTurnEnd(ulong clientId)
        {
            return _isTurnEndList.OnChangedAsObservable()
                .Where(e => e.Value.ClientId == clientId)
                .Do(e => Log.Info("ListEvent: {0}", e.Type.ToString()))
                .Where(e => e.Type == NetworkListEvent<ClientTurn>.EventType.Value)
                .Select(e => e.Value.IsTurnEnd);
        }

        public void AddPlayer(ulong clientId)
        {
            if (NetworkManager.ServerClientId == clientId) return;
            _isTurnEndList.Add(new ClientTurn(clientId, false));
        }

        public void RemovePlayer(ulong clientId)
        {
            var index = _isTurnEndList.IndexOfClientId(clientId);
            if (index == -1) return;

            _isTurnEndList.RemoveAt(index);
            CheckTurnEndAllPlayer(); // プレイヤーが抜けた時に全員がターン終了しているか確認
        }

        [Rpc(SendTo.Server)]
        public void ToggleTurnEndRpc(ulong clientId)
        {
            var index = _isTurnEndList.IndexOfClientId(clientId);
            if (index == -1)
            {
                Log.Warning("Player not found. PlayerId: {0}", clientId);
                return;
            }

            _isTurnEndList[index] = new ClientTurn(clientId, !_isTurnEndList[index].IsTurnEnd);
            CheckTurnEndAllPlayer();
        }

        private void CheckTurnEndAllPlayer()
        {
            foreach (var playerTurn in _isTurnEndList)
                if (!playerTurn.IsTurnEnd)
                    return;
            _onTurnEnd.OnNext(Unit.Default);

            // ターン終了フラグをリセット
            for (var i = 0; i < _isTurnEndList.Count; i++)
                _isTurnEndList[i] = new ClientTurn(_isTurnEndList[i].ClientId, false);
        }

        private struct ClientTurn : INetworkSerializable, IEquatable<ClientTurn>, IDataWithClientId
        {
            private ulong _clientId;
            private bool _isTurnEnd;

            public ClientTurn(ulong clientId, bool isTurnEnd)
            {
                _clientId = clientId;
                _isTurnEnd = isTurnEnd;
            }

            public ulong ClientId => _clientId;
            public bool IsTurnEnd => _isTurnEnd;

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref _clientId);
                serializer.SerializeValue(ref _isTurnEnd);
            }

            public bool Equals(ClientTurn other)
            {
                return _clientId == other._clientId && _isTurnEnd == other._isTurnEnd;
            }

            public override bool Equals(object obj)
            {
                return obj is ClientTurn other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(_clientId, _isTurnEnd);
            }
        }
    }
}