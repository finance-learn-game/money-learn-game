using System;
using Unity.Netcode;

namespace SmbcApp.LearnGame.Infrastructure
{
    public readonly struct NetworkGuid : INetworkSerializeByMemcpy, IEquatable<NetworkGuid>
    {
        public readonly ulong FirstHalf, SecondHalf;

        public NetworkGuid(ulong firstHalf, ulong secondHalf)
        {
            FirstHalf = firstHalf;
            SecondHalf = secondHalf;
        }

        public override string ToString()
        {
            return $"[{FirstHalf}, {SecondHalf}]";
        }

        public static bool operator ==(NetworkGuid a, NetworkGuid b)
        {
            return a.FirstHalf == b.FirstHalf && a.SecondHalf == b.SecondHalf;
        }

        public static bool operator !=(NetworkGuid a, NetworkGuid b)
        {
            return !(a == b);
        }

        public bool Equals(NetworkGuid other)
        {
            return FirstHalf == other.FirstHalf && SecondHalf == other.SecondHalf;
        }

        public override bool Equals(object obj)
        {
            return obj is NetworkGuid other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(FirstHalf, SecondHalf);
        }
    }

    public static class NetworkGuidExtensions
    {
        public static NetworkGuid ToNetworkGuid(this Guid value)
        {
            var bytes = value.ToByteArray();
            var networkId = new NetworkGuid(
                BitConverter.ToUInt64(bytes, 0),
                BitConverter.ToUInt64(bytes, 8)
            );
            return networkId;
        }

        public static Guid ToGuid(this NetworkGuid value)
        {
            var bytes = new byte[16];
            BitConverter.GetBytes(value.FirstHalf).CopyTo(bytes, 0);
            BitConverter.GetBytes(value.SecondHalf).CopyTo(bytes, 8);
            return new Guid(bytes);
        }
    }
}