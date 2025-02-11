using MasterMemory;
using MessagePack;
using MessagePack.Resolvers;
using UnityEngine;

namespace SmbcApp.LearnGame.Data
{
    internal static class Initializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void SetupMessagePackResolver()
        {
            StaticCompositeResolver.Instance.Register(MasterMemoryResolver.Instance, StandardResolver.Instance);

            var options = MessagePackSerializerOptions.Standard.WithResolver(StaticCompositeResolver.Instance);
            MessagePackSerializer.DefaultOptions = options;
        }
    }
}