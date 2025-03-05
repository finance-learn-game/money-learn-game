using System;
using R3;
using SmbcApp.LearnGame.GamePlay.GamePlayObjects;
using SmbcApp.LearnGame.Infrastructure;
using Unity.Netcode;

namespace SmbcApp.LearnGame.GamePlay.GameState.NetworkData
{
    /// <summary>
    ///     AvatarSelectシーンで共通のデータとRPC
    /// </summary>
    internal sealed class NetworkAvatarSelection : NetworkBehaviour
    {
        private readonly Subject<ChangeStateParams> _onStateChange = new();
        public NetworkList<SessionPlayerState> SessionPlayers { get; private set; }

        /// <summary>
        ///     全員のアバター選択が完了し、フラグがReadyになったかどうか
        /// </summary>
        public NetworkVariable<bool> IsAvatarSelectFinished { get; } = new();

        /// <summary>
        ///     準備が完了したクライアントを通知する
        /// </summary>
        public Observable<ChangeStateParams> OnStateChange => _onStateChange;

        private void Awake()
        {
            SessionPlayers = new NetworkList<SessionPlayerState>();
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        public void ServerChangeAvatarStateRpc(ulong clientId, NetworkGuid avatar)
        {
            _onStateChange.OnNext(new ChangeStateParams
            {
                ClientId = clientId,
                IsReady = null,
                Avatar = avatar
            });
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        public void ServerChangeReadyStateRpc(ulong clientId, bool isReady)
        {
            _onStateChange.OnNext(new ChangeStateParams
            {
                ClientId = clientId,
                IsReady = isReady,
                Avatar = null
            });
        }

        public record ChangeStateParams
        {
            public ulong ClientId { get; init; }
            public bool? IsReady { get; init; }
            public NetworkGuid? Avatar { get; init; }
        }

        /// <summary>
        ///     セッション内のプレイヤーと、プレイヤーの準備状態を表す構造体
        /// </summary>
        public struct SessionPlayerState : INetworkSerializable, IEquatable<SessionPlayerState>
        {
            public ulong ClientId;
            public bool IsReady;
            public NetworkGuid AvatarGuid;
            public int PlayerNumber;
            public FixedPlayerName PlayerName;

            public SessionPlayerState(
                ulong clientId,
                in FixedPlayerName playerName,
                bool isReady = false,
                NetworkGuid avatarGuid = default,
                int playerNumber = -1
            )
            {
                ClientId = clientId;
                IsReady = isReady;
                AvatarGuid = avatarGuid;
                PlayerNumber = playerNumber;
                PlayerName = playerName;
            }

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref ClientId);
                serializer.SerializeValue(ref IsReady);
                serializer.SerializeValue(ref AvatarGuid);
                serializer.SerializeValue(ref PlayerNumber);
                serializer.SerializeValue(ref PlayerName);
            }

            public bool Equals(SessionPlayerState other)
            {
                return ClientId == other.ClientId &&
                       IsReady == other.IsReady &&
                       AvatarGuid == other.AvatarGuid &&
                       PlayerNumber == other.PlayerNumber &&
                       PlayerName.Equals(other.PlayerName);
            }
        }
    }
}