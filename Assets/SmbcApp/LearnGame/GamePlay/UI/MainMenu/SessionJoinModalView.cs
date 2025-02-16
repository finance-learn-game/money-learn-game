using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using R3;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.UIWidgets.Button;
using SmbcApp.LearnGame.UIWidgets.Modal;
using TMPro;
using UnityEngine;
using VContainer;

namespace SmbcApp.LearnGame.Gameplay.UI.MainMenu
{
    internal sealed class SessionJoinModalView : UIModal
    {
        [SerializeField] [Required] private TMP_InputField sessionCodeInputField;
        [SerializeField] [Required] private UIButton joinButton;
        [SerializeField] [Required] private TMP_Text messageText;

        [Inject] internal MainMenuUIMediator MainMenuUIMediator;

        protected override void Start()
        {
            base.Start();

            OnSessionCodeChanged(string.Empty);
            sessionCodeInputField.onValueChanged.AddListener(OnSessionCodeChanged);
            joinButton.OnClick
                .SubscribeAwait(OnJoinButtonClicked)
                .AddTo(gameObject);
        }

        private void OnDestroy()
        {
            sessionCodeInputField.onValueChanged.RemoveListener(OnSessionCodeChanged);
        }

        private async ValueTask OnJoinButtonClicked(Unit _, CancellationToken cancellation = new())
        {
            var joinCode = SanitizeJoinCode(sessionCodeInputField.text);
            try
            {
                if (!await MainMenuUIMediator.JoinSessionWithCode(joinCode))
                    throw new Exception("Failed to join session.");
            }
            catch (Exception)
            {
                messageText.text = "セッションに参加できませんでした。";
            }
        }

        private void OnSessionCodeChanged(string value)
        {
            sessionCodeInputField.text = SanitizeJoinCode(value);
            joinButton.IsInteractable = !string.IsNullOrEmpty(value);
            messageText.text = string.Empty;
        }

        private static string SanitizeJoinCode(string dirtyString)
        {
            return Regex.Replace(dirtyString.ToUpper(), "[^A-Z0-9]", "");
        }
    }
}