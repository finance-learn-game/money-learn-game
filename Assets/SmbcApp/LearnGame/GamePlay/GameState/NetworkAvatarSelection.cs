using System;
using R3;
using SmbcApp.LearnGame.GamePlay.GamePlayObjects;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;
using Avatar = SmbcApp.LearnGame.GamePlay.Configuration.Avatar;
using SeatChangeEvent = System.ValueTuple<ulong, int, bool>;

namespace SmbcApp.LearnGame.Gameplay.GameState
{
    /// <summary>
    ///     AvatarSelectシーンで共通のデータとRPC
    /// </summary>
    internal sealed class NetworkAvatarSelection : NetworkBehaviour
    {
        public enum SeatState : byte
        {
            Inactive,
            Active,
            LockedIn
        }

        [SerializeField] private Avatar[] avatarConfiguration;

        private readonly Subject<SeatChangeEvent> _onPlayerSeatChange = new();

        public NetworkList<SessionPlayerState> SessionPlayers { get; private set; }
        public ReadOnlyArray<Avatar> AvatarConfiguration => avatarConfiguration;

        /// <summary>
        ///     全員のアバター選択が完了し、ゲームを開始したとき
        /// </summary>
        public NetworkVariable<bool> IsAvatarSelectFinished { get; } = new();

        public Observable<SeatChangeEvent> OnPlayerSeatChange => _onPlayerSeatChange;

        private void Awake()
        {
            SessionPlayers = new NetworkList<SessionPlayerState>();
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        public void ServerChangeSeatRpc(ulong clientId, int seadIdx, bool lockedIn)
        {
            _onPlayerSeatChange.OnNext((clientId, seadIdx, lockedIn));
        }

        /// <summary>
        ///     セッション内のプレイヤーと、プレイヤーが選んだアバターを表すデータ
        /// </summary>
        public struct SessionPlayerState : INetworkSerializable, IEquatable<SessionPlayerState>
        {
            public ulong ClientId;
            public int PlayerNumber;
            public int SeatIdx;
            public float LastChangeTime;
            public SeatState SeatState;

            private FixedPlayerName _playerName;

            public SessionPlayerState(
                ulong clientId,
                string name,
                int playerNumber,
                SeatState state,
                int seatIdx = -1,
                float lastChangeTime = 0
            )
            {
                ClientId = clientId;
                PlayerNumber = playerNumber;
                SeatState = state;
                SeatIdx = seatIdx;
                LastChangeTime = lastChangeTime;
                _playerName = new FixedPlayerName();

                PlayerName = name;
            }

            public string PlayerName
            {
                get => _playerName;
                private set => _playerName = value;
            }

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref ClientId);
                serializer.SerializeValue(ref _playerName);
                serializer.SerializeValue(ref PlayerNumber);
                serializer.SerializeValue(ref SeatState);
                serializer.SerializeValue(ref SeatIdx);
                serializer.SerializeValue(ref LastChangeTime);
            }

            public bool Equals(SessionPlayerState other)
            {
                return ClientId == other.ClientId &&
                       _playerName.Equals(other._playerName) &&
                       PlayerNumber == other.PlayerNumber &&
                       SeatIdx == other.SeatIdx &&
                       LastChangeTime.Equals(other.LastChangeTime) &&
                       SeatState == other.SeatState;
            }
        }
    }
}