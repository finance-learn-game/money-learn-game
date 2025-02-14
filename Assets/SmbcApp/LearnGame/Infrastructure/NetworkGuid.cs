using System;
using Unity.Netcode;

namespace SmbcApp.LearnGame.Infrastructure
{
    public readonly struct NetworkGuid : INetworkSerializeByMemcpy
    {
        public readonly ulong FirstHalf, SecondHalf;

        public NetworkGuid(ulong firstHalf, ulong secondHalf)
        {
            FirstHalf = firstHalf;
            SecondHalf = secondHalf;
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

        public static NetworkGuid ToNetworkGuid(this string value)
        {
            return Guid.Parse(value).ToNetworkGuid();
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