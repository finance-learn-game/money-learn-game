using System;
using R3;
using SmbcApp.LearnGame.Infrastructure;
using Unity.Netcode;

namespace SmbcApp.LearnGame.Gameplay.GameState
{
    /// <summary>
    ///     AvatarSelectシーンで共通のデータとRPC
    /// </summary>
    internal sealed class NetworkAvatarSelection : NetworkBehaviour
    {
        private readonly Subject<SessionPlayerState> _onStateChange = new();
        public NetworkList<SessionPlayerState> SessionPlayers { get; private set; }

        /// <summary>
        ///     全員のアバター選択が完了し、フラグがReadyになったかどうか
        /// </summary>
        public NetworkVariable<bool> IsAvatarSelectFinished { get; } = new();

        /// <summary>
        ///     準備が完了したクライアントを通知する
        /// </summary>
        public Observable<SessionPlayerState> OnStateChange => _onStateChange;

        private void Awake()
        {
            SessionPlayers = new NetworkList<SessionPlayerState>();
        }

        /// <summary>
        ///     サーバーに状態の変化を通知するRPC
        /// </summary>
        /// <param name="clientId">準備状態を通知するクライアントID</param>
        /// <param name="isReady">準備状態</param>
        /// <param name="avatar">アバター</param>
        [Rpc(SendTo.Server, RequireOwnership = false)]
        public void ServerChangeStateRpc(ulong clientId, bool isReady, NetworkGuid avatar)
        {
            _onStateChange.OnNext(new SessionPlayerState(clientId, isReady, avatar));
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

            public SessionPlayerState(
                ulong clientId,
                bool isReady = false,
                NetworkGuid avatarGuid = default,
                int playerNumber = -1
            )
            {
                ClientId = clientId;
                IsReady = isReady;
                AvatarGuid = avatarGuid;
                PlayerNumber = playerNumber;
            }

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref ClientId);
                serializer.SerializeValue(ref IsReady);
                serializer.SerializeValue(ref AvatarGuid);
                serializer.SerializeValue(ref PlayerNumber);
            }

            public bool Equals(SessionPlayerState other)
            {
                return ClientId == other.ClientId &&
                       IsReady == other.IsReady &&
                       AvatarGuid == other.AvatarGuid &&
                       PlayerNumber == other.PlayerNumber;
            }
        }
    }
}