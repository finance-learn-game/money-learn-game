using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePack;
using R3;
using Unity.Logging;
using UnityEngine;
using VContainer.Unity;

namespace SmbcApp.LearnGame.Data
{
    public class SaveDataService : IAsyncStartable, IDisposable
    {
        private const string SaveDataFileName = "SaveData.bin";

        private readonly ReactiveProperty<GameSaveData> _saveData = new();
        private readonly string _saveDataPath = Path.Combine(Application.persistentDataPath, SaveDataFileName);
        private bool _isDirty;

        public ReadOnlyReactiveProperty<GameSaveData> SaveData => _saveData;
        public bool Initialized { get; private set; }

        public async UniTask StartAsync(CancellationToken cancellation = new())
        {
            await Load(cancellation);
            Initialized = true;
        }

        public void Dispose()
        {
            _saveData?.Dispose();
        }

        public void ChangeSaveData(Func<GameSaveData, GameSaveData> change)
        {
            _saveData.Value = change(_saveData.Value);
            _isDirty = true;
        }

        public async UniTask AsyncDispose(CancellationToken cancellation = new())
        {
            await Save(cancellation);
        }

        private async UniTask Load(CancellationToken cancellation = new())
        {
            if (!File.Exists(_saveDataPath))
            {
                _saveData.Value = GameSaveData.Default;
                _isDirty = true;
                await Save(cancellation);
                Log.Info("Save data initialized");
                return;
            }

            await using var fileStream = new FileStream(_saveDataPath, FileMode.Open);
            try
            {
                var loadedData =
                    await MessagePackSerializer.Typeless.DeserializeAsync(fileStream, cancellationToken: cancellation);
                if (loadedData is not GameSaveData data) throw new Exception("Failed to load save data");

                _saveData.Value = data;
#if UNITY_EDITOR
                Log.Info("Loaded save data: {0}", _saveData.ToString());
#endif
                Log.Info("Save data loaded from {0}", _saveDataPath);
            }
            catch (Exception)
            {
#if UNITY_EDITOR
                Log.Error("Failed to load save data, deleting save data file");
                File.Delete(_saveDataPath);
                _saveData.Value = GameSaveData.Default;
#endif
                throw;
            }
        }

        private async UniTask Save(CancellationToken cancellation = new())
        {
            if (!_isDirty)
            {
                Log.Info("Save data is not dirty, skipping save");
                return;
            }

            await using var fileStream = new FileStream(_saveDataPath, FileMode.Create);
            await MessagePackSerializer.Typeless.SerializeAsync(fileStream, _saveData.Value,
                cancellationToken: cancellation);
            _isDirty = false;
            Log.Info("Save data saved to {0}", _saveDataPath);
        }
    }
}