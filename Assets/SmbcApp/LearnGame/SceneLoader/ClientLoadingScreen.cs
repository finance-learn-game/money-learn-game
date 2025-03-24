using System;
using System.Collections;
using System.Collections.Generic;
using R3;
using SmbcApp.LearnGame.UIWidgets.Slider;
using TMPro;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Enumerable = System.Linq.Enumerable;

namespace SmbcApp.LearnGame.SceneLoader
{
    public abstract class ClientLoadingScreen : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float delayBeforeFadeOut = 0.5f;
        [SerializeField] private float fadeOutDuration = 0.1f;
        [SerializeField] private Slider progressBar;
        [SerializeField] private TMP_Text sceneNameText;
        [SerializeField] private UINoInteractSlider[] otherPlayersProgressBars;
        [SerializeField] protected LoadingProgressManager loadingProgressManager;

        protected readonly Dictionary<ulong, LoadingProgressBar> LoadingProgressBars = new();

        private Coroutine _fadeOutCoroutine;
        private bool _loadingScreenRunning;

        private void Awake()
        {
            DontDestroyOnLoad(this);
        }

        private void Start()
        {
            SetCanvasVisibility(false);
            loadingProgressManager.OnTrackersUpdated
                .Subscribe(OnProgressTrackersUpdated)
                .AddTo(gameObject);
        }

        private void Update()
        {
            if (_loadingScreenRunning) progressBar.value = loadingProgressManager.LocalProgress;
        }

        private void OnProgressTrackersUpdated(Unit _)
        {
            // 追跡されていないクライアントのプログレスバーを非アクティブにする
            var clientIdsToRemove = Enumerable.ToList(Enumerable.Where(LoadingProgressBars.Keys,
                clientId => !loadingProgressManager.ProgressTrackers.ContainsKey(clientId)));
            foreach (var clientId in clientIdsToRemove) RemoveOtherPlayerProgressBar(clientId);

            // トラッキングされていないクライアントのプログレスバーを追加する
            foreach (var (clientId, tracker) in loadingProgressManager.ProgressTrackers)
                if (clientId != NetworkManager.Singleton.LocalClientId && !LoadingProgressBars.ContainsKey(clientId))
                    AddOtherPlayerProgressBar(clientId, tracker);
        }

        public void StopLoadingScreen()
        {
            if (!_loadingScreenRunning) return;

            if (_fadeOutCoroutine != null) StopCoroutine(_fadeOutCoroutine);
            _fadeOutCoroutine = StartCoroutine(FadeOutCoroutine());
        }

        public void StartLoadingScreen(string sceneName)
        {
            SetCanvasVisibility(true);
            _loadingScreenRunning = true;
            UpdateLoadingScreen(sceneName);
            ReinitializeProgressBars();
        }

        private void ReinitializeProgressBars()
        {
            // 追跡されていないクライアントのプログレスバーを非アクティブにする
            var clientIdsToRemove = Enumerable.ToList(Enumerable.Where(LoadingProgressBars.Keys,
                clientId => !loadingProgressManager.ProgressTrackers.ContainsKey(clientId)));
            foreach (var clientId in clientIdsToRemove) RemoveOtherPlayerProgressBar(clientId);
            foreach (var slider in otherPlayersProgressBars) slider.gameObject.SetActive(false);

            foreach (
                var (clientId, idx) in Enumerable.Select(
                    Enumerable.Where(loadingProgressManager.ProgressTrackers.Keys,
                        clientId => clientId != NetworkManager.Singleton.LocalClientId),
                    (clientId, idx) => (clientId, idx))
            )
                UpdateOtherPlayerProgressBar(clientId, idx);
        }

        protected virtual void UpdateOtherPlayerProgressBar(ulong clientId, int progressBarIndex)
        {
            LoadingProgressBars[clientId].ProgressBar = otherPlayersProgressBars[progressBarIndex];
            LoadingProgressBars[clientId].ProgressBar.gameObject.SetActive(true);
        }

        protected virtual void AddOtherPlayerProgressBar(
            ulong clientId,
            NetworkedLoadingProgressTracker progressTracker
        )
        {
            if (LoadingProgressBars.Count < otherPlayersProgressBars.Length)
            {
                var index = LoadingProgressBars.Count;
                LoadingProgressBars[clientId] = new LoadingProgressBar(otherPlayersProgressBars[index]);
                progressTracker.Progress.OnValueChanged += LoadingProgressBars[clientId].UpdateProgress;
                LoadingProgressBars[clientId].Value = progressTracker.Progress.Value;
                LoadingProgressBars[clientId].NameText = $"Client {clientId}";
                LoadingProgressBars[clientId].ProgressBar.gameObject.SetActive(true);
            }
            else
            {
                throw new Exception("There are not enough progress bars to track the progress of all the players.");
            }
        }

        private void RemoveOtherPlayerProgressBar(
            ulong clientId,
            NetworkedLoadingProgressTracker progressTracker = null
        )
        {
            if (progressTracker != null)
                progressTracker.Progress.OnValueChanged -= LoadingProgressBars[clientId].UpdateProgress;
            LoadingProgressBars[clientId].ProgressBar.gameObject.SetActive(false);
            LoadingProgressBars.Remove(clientId);
        }

        public void UpdateLoadingScreen(string sceneName)
        {
            if (!_loadingScreenRunning) return;

            sceneNameText.text = sceneName;
            if (_fadeOutCoroutine != null) StopCoroutine(_fadeOutCoroutine);
        }

        private void SetCanvasVisibility(bool visible)
        {
            if (!Application.isPlaying) return;
            canvasGroup.alpha = visible ? 1 : 0;
            canvasGroup.blocksRaycasts = visible;
        }

        private IEnumerator FadeOutCoroutine()
        {
            yield return new WaitForSeconds(delayBeforeFadeOut);
            _loadingScreenRunning = false;

            float currentTime = 0;
            while (currentTime < fadeOutDuration)
            {
                canvasGroup.alpha = math.lerp(1, 0, currentTime / fadeOutDuration);
                yield return null;
                currentTime += Time.deltaTime;
            }

            SetCanvasVisibility(false);
        }

        protected class LoadingProgressBar
        {
            public LoadingProgressBar(UINoInteractSlider progressBar)
            {
                ProgressBar = progressBar;
            }

            public UINoInteractSlider ProgressBar { get; set; }

            public string NameText
            {
                get => ProgressBar.Label;
                set => ProgressBar.Label = value;
            }

            public float Value
            {
                get => ProgressBar.Value;
                set => ProgressBar.Value = value;
            }

            public void UpdateProgress(float value, float newValue)
            {
                ProgressBar.Value = newValue;
            }
        }
    }
}