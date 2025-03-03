using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using MasterMemory;
using Unity.Logging;
using VContainer;
using VContainer.Unity;

namespace SmbcApp.LearnGame.Data
{
    public sealed class MasterData : IAsyncStartable
    {
        public const string BinaryDirectory = "Assets/Projects/MasterData/Binary";
        public const string BinaryFileName = "MasterData.bin";

        [Inject]
        public MasterData()
        {
        }

        public MemoryDatabase DB { get; private set; }
        public bool Initialized { get; private set; }


        public async UniTask StartAsync(CancellationToken cancellation = new())
        {
            DB = await LoadDB(cancellation);
            Initialized = true;
            Log.Info("MasterData loaded.");
        }

        private static async UniTask<MemoryDatabase> LoadDB(CancellationToken cancellation = new())
        {
            await using var stream = new FileStream(Path.Combine(BinaryDirectory, BinaryFileName), FileMode.Open);
            var buffer = new byte[stream.Length];
            _ = await stream.ReadAsync(buffer, cancellation);
            var db = new MemoryDatabase(buffer);
            return db;
        }
    }
}