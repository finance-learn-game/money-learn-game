using System;
using System.IO;
using SmbcApp.LearnGame.SceneLoader;
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
    internal static class InitializeOnLoadApp
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeOnLoad()
        {
            ConfigureLogger();

#if UNITY_EDITOR
            if (SceneInitializerEditor.IsLoadStartUp())
                SceneManager.LoadScene(AppScenes.StartUp);
            else
                Log.Warning(
                    "Now you are in the Editor. If you want to load the startup scene, please select the menu item.");
#endif
        }

        private static void ConfigureLogger()
        {
            Log.Logger = new Logger(
#if UNITY_EDITOR
                EditorConfiguration()
#elif DEBUG
                DevelopmentConfiguration()
#else
                ReleaseConfiguration()
#endif
            );
        }

        private static LoggerConfig EditorConfiguration()
        {
            return new LoggerConfig()
                .SyncMode.FatalIsSync()
                .RedirectUnityLogs()
                .WriteTo.UnityEditorConsole(
                    minLevel: LogLevel.Info,
                    captureStackTrace: true
                );
        }

        private static LoggerConfig DevelopmentConfiguration()
        {
            var random = Guid.NewGuid().ToString()[..8];
            var path = Path.Combine(
                Directory.GetCurrentDirectory(),
                $"Logs/dev-{random}",
                $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log"
            );

            return new LoggerConfig()
                .SyncMode.FatalIsSync()
                .RedirectUnityLogs()
                .WriteTo.File(
                    path,
                    minLevel: LogLevel.Debug,
                    captureStackTrace: true,
                    outputTemplate: "{Timestamp} [{Level}] {Message}{NewLine}{Stacktrace}"
                );
        }

        private static LoggerConfig ReleaseConfiguration()
        {
            var random = Guid.NewGuid().ToString()[..8];
            var path = Path.Combine(
                Directory.GetCurrentDirectory(),
                $"Logs/dev-{random}",
                $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log"
            );

            return new LoggerConfig()
                .SyncMode.FatalIsSync()
                .RedirectUnityLogs()
                .WriteTo.File(
                    path,
                    minLevel: LogLevel.Info,
                    captureStackTrace: false,
                    outputTemplate: "{Timestamp} [{Level}] {Message}{NewLine}"
                );
        }

#if UNITY_EDITOR
        [InitializeOnEnterPlayMode]
        private static void InitializeOnEnterPlayMode()
        {
            if (SceneInitializerEditor.IsLoadStartUp())
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

#if UNITY_EDITOR
    public static class SceneInitializerEditor
    {
        private const string MenuPath = "Tools/Auto Load Startup Scene";
        private const string LoadStartUpItemKey = "SceneInitializerEditor.LoadStartUp";

        [MenuItem(MenuPath)]
        private static void LoadStartUp()
        {
            EditorPrefs.SetBool(LoadStartUpItemKey, !EditorPrefs.GetBool(LoadStartUpItemKey, false));
        }

        [MenuItem(MenuPath, true)]
        private static bool LoadStartUpValidate()
        {
            Menu.SetChecked(MenuPath, EditorPrefs.GetBool(LoadStartUpItemKey, false));
            return true;
        }

        public static bool IsLoadStartUp()
        {
            return EditorPrefs.GetBool(LoadStartUpItemKey, false);
        }
    }
#endif
}