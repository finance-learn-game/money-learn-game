using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using MasterMemory;
using Unity.Logging;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;
using VContainer.Unity;

namespace SmbcApp.LearnGame.Data
{
    public sealed class MasterData : IAsyncStartable
    {
        public const string BinaryDirectory = "Assets/Projects/MasterData/Binary";
        public const string BinaryFileName = "MasterData.bytes";

        [Inject]
        public MasterData()
        {
        }

        public MemoryDatabase DB { get; private set; }
        public bool Initialized { get; private set; }


        public async UniTask StartAsync(CancellationToken cancellation = new())
        {
            DB = await LoadDB();
            Initialized = true;
            Log.Info("MasterData loaded.");
        }

        private static async UniTask<MemoryDatabase> LoadDB()
        {
            var handle = Addressables.LoadAssetAsync<TextAsset>(Path.GetFileNameWithoutExtension(BinaryFileName));
            var db = new MemoryDatabase((await handle).bytes);
            Addressables.Release(handle);
            return db;
        }
    }
}