using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using R3;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace SmbcApp.LearnGame.Utils
{
    public abstract class ClientLoadingScreen : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float delayBeforeFadeOut = 0.5f;
        [SerializeField] private float fadeOutDuration = 0.1f;
        [SerializeField] private Slider progressBar;
        [SerializeField] private TMP_Text sceneNameText;
        [SerializeField] private List<TMP_Text> otherPlayerNamesTexts;
        [SerializeField] private List<Slider> otherPlayersProgressBars;
        [SerializeField] protected LoadingProgressManager loadingProgressManager;

        protected readonly Dictionary<ulong, LoadingProgressBar> LoadingProgressBars = new();

        private Coroutine _fadeOutCoroutine;
        private bool _loadingScreenRunning;

        private void Awake()
        {
            DontDestroyOnLoad(this);
            Assert.AreEqual(otherPlayersProgressBars.Count, otherPlayerNamesTexts.Count,
                "There should be the same number of progress bars and name labels");
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
            var clientIdsToRemove = LoadingProgressBars.Keys
                .Where(clientId => !loadingProgressManager.ProgressTrackers.ContainsKey(clientId)).ToList();
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
            var clientIdsToRemove = LoadingProgressBars.Keys
                .Where(clientId => !loadingProgressManager.ProgressTrackers.ContainsKey(clientId)).ToList();
            foreach (var clientId in clientIdsToRemove) RemoveOtherPlayerProgressBar(clientId);

            for (var i = 0; i < otherPlayersProgressBars.Count; i++)
            {
                otherPlayersProgressBars[i].gameObject.SetActive(false);
                otherPlayerNamesTexts[i].gameObject.SetActive(false);
            }

            foreach (
                var (clientId, idx) in loadingProgressManager.ProgressTrackers
                    .Select(progressTracker => progressTracker.Key)
                    .Where(clientId => clientId != NetworkManager.Singleton.LocalClientId)
                    .Select((clientId, idx) => (clientId, idx))
            )
                UpdateOtherPlayerProgressBar(clientId, idx);
        }

        protected virtual void UpdateOtherPlayerProgressBar(ulong clientId, int progressBarIndex)
        {
            LoadingProgressBars[clientId].ProgressBar = otherPlayersProgressBars[progressBarIndex];
            LoadingProgressBars[clientId].ProgressBar.gameObject.SetActive(true);
            LoadingProgressBars[clientId].NameText = otherPlayerNamesTexts[progressBarIndex];
            LoadingProgressBars[clientId].NameText.gameObject.SetActive(true);
        }

        protected virtual void AddOtherPlayerProgressBar(
            ulong clientId,
            NetworkedLoadingProgressTracker progressTracker
        )
        {
            if (LoadingProgressBars.Count < otherPlayersProgressBars.Count &&
                LoadingProgressBars.Count < otherPlayerNamesTexts.Count)
            {
                var index = LoadingProgressBars.Count;
                LoadingProgressBars[clientId] =
                    new LoadingProgressBar(otherPlayersProgressBars[index], otherPlayerNamesTexts[index]);
                progressTracker.Progress.OnValueChanged += LoadingProgressBars[clientId].UpdateProgress;
                LoadingProgressBars[clientId].ProgressBar.value = progressTracker.Progress.Value;
                LoadingProgressBars[clientId].ProgressBar.gameObject.SetActive(true);
                LoadingProgressBars[clientId].NameText.gameObject.SetActive(true);
                LoadingProgressBars[clientId].NameText.text = $"Client {clientId}";
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
            LoadingProgressBars[clientId].NameText.gameObject.SetActive(false);
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
                canvasGroup.alpha = Mathf.Lerp(1, 0, currentTime / fadeOutDuration);
                yield return null;
                currentTime += Time.deltaTime;
            }

            SetCanvasVisibility(false);
        }

        protected class LoadingProgressBar
        {
            public LoadingProgressBar(Slider otherPlayerProgressBar, TMP_Text otherPlayerNameText)
            {
                ProgressBar = otherPlayerProgressBar;
                NameText = otherPlayerNameText;
            }

            public Slider ProgressBar { get; set; }

            public TMP_Text NameText { get; set; }

            public void UpdateProgress(float value, float newValue)
            {
                ProgressBar.value = newValue;
            }
        }
    }
}