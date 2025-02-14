using SmbcApp.LearnGame.Utils;
using Unity.Logging;
using Unity.Logging.Sinks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = Unity.Logging.Logger;

namespace SmbcApp.LearnGame.ApplicationLifecycle
{
    /// <summary>
    ///     アプリケーション内のInitializeOnxxx処理をまとめる
    /// </summary>
    public static class InitializeOnLoadApp
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeOnLoad()
        {
            ConfigureLogger();

#if UNITY_EDITOR
            SceneManager.LoadScene(AppScenes.StartUp);
#endif
        }

        private static void ConfigureLogger()
        {
            Log.Logger = new Logger(EditorConfiguration());
            return;

            static LoggerConfig EditorConfiguration()
            {
                return new LoggerConfig()
                    .SyncMode.FatalIsSync()
                    .RedirectUnityLogs()
                    .WriteTo.UnityEditorConsole(
                        minLevel: LogLevel.Info,
                        captureStackTrace: true
                    );
            }
        }

#if UNITY_EDITOR
        [InitializeOnEnterPlayMode]
        private static void InitializeOnEnterPlayMode()
        {
            SetActiveRootGameObjects(false);
        }

        private static void SetActiveRootGameObjects(bool active)
        {
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.name == AppScenes.StartUp) continue;
                var rootGameObjects = scene.GetRootGameObjects();
                foreach (var rootGameObject in rootGameObjects) rootGameObject.SetActive(active);
            }
        }
#endif
    }
}