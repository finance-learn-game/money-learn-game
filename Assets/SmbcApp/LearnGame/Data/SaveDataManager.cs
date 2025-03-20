using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using MessagePack;
using R3;
using Unity.Logging;
using UnityEngine;
using VContainer.Unity;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace SmbcApp.LearnGame.Data
{
    /// <summary>
    ///     セーブデータの管理を行うクラス
    /// </summary>
    public sealed class SaveDataManager : IAsyncStartable, IAsyncDisposable
    {
        private const string SaveDataFileName = "SaveData.bin";

        private static string _saveDataPath;

        private readonly ReactiveProperty<GameSaveData> _saveData = new();

        public bool IsDirty { get; private set; }

        public ReadOnlyReactiveProperty<GameSaveData> SaveData => _saveData;
        public bool Initialized { get; private set; }

        /// <summary>
        ///     ゲーム終了時にセーブデータを保存する用途で使用する
        /// </summary>
        /// <remarks>ApplicationLifecycleから呼ばれる</remarks>
        public async ValueTask DisposeAsync()
        {
            await Save();
            _saveData.Dispose();
        }

        public async UniTask StartAsync(CancellationToken cancellation = new())
        {
            await Load(cancellation);
            Initialized = true;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeSaveDataPath()
        {
            _saveDataPath = Path.Combine(Directory.GetCurrentDirectory(), SaveDataFileName);

            Debug.Log($"[SaveDataManager] Save data path: {_saveDataPath}");
        }

        public void ChangeSaveData(Func<GameSaveData, GameSaveData> change)
        {
            _saveData.Value = change(_saveData.Value);
            IsDirty = true;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        private UniTask Load(CancellationToken cancellation = new()) {
            _saveData.Value = GameSaveData.Default;
            return UniTask.CompletedTask;
        }
#else
        private async UniTask Load(CancellationToken cancellation = new())
        {
            if (!File.Exists(_saveDataPath))
            {
                _saveData.Value = GameSaveData.Default;
                IsDirty = true;
                await Save(cancellation);
                Log.Info("Save data initialized");
                return;
            }

            try
            {
                await using var fileStream = new FileStream(_saveDataPath, FileMode.Open);
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
#else
                _saveData.Value = GameSaveData.Default;
                Log.Error("Failed to load save data");
#endif
                throw;
            }
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        private UniTask Save(CancellationToken cancellation = new())
        {
            return UniTask.CompletedTask;
        }
#else
        private async UniTask Save(CancellationToken cancellation = new())
        {
            if (!IsDirty)
            {
                Log.Info("Save data is not dirty, skipping save");
                return;
            }

            await using var fileStream = new FileStream(_saveDataPath, FileMode.Create);
            await MessagePackSerializer.Typeless.SerializeAsync(fileStream, _saveData.Value,
                cancellationToken: cancellation);
            IsDirty = false;
            Log.Info("Save data saved to {0}", _saveDataPath);
        }
#endif
    }
}