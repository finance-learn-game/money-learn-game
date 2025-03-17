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

        private readonly ReactiveProperty<GameSaveData> _saveData = new();

        private static string _saveDataPath;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeSaveDataPath()
        {
            _saveDataPath = Path.Combine(Directory.GetCurrentDirectory(), SaveDataFileName);
            Debug.Log($"[SaveDataManager] Save data path: {_saveDataPath}");
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        private readonly string _saveDataPath = Path.Combine(Application.persistentDataPath, SaveDataFileName);
#endif
        public bool IsDirty { get; private set; }

        public ReadOnlyReactiveProperty<GameSaveData> SaveData => _saveData;
        public bool Initialized { get; private set; }

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void syncDB();
#endif

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

        public void ChangeSaveData(Func<GameSaveData, GameSaveData> change)
        {
            _saveData.Value = change(_saveData.Value);
            IsDirty = true;
        }

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
#endif
                throw;
            }
        }

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
#if UNITY_WEBGL && !UNITY_EDITOR
            syncDB();
#endif
            Log.Info("Save data saved to {0}", _saveDataPath);
        }
    }
}