using R3;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.UIWidgets.Button;
using SmbcApp.LearnGame.UnityService.Session;
using TMPro;
using UnityEngine;
using VContainer;

namespace SmbcApp.LearnGame.GamePlay.UI.AvatarSelect
{
    internal sealed class SessionCodeView : MonoBehaviour
    {
        [SerializeField] [Required] private TMP_Text sessionCodeText;
        [SerializeField] [Required] private UIButton copyButton;

        private void Start()
        {
            copyButton.OnClick.Subscribe(_ => GUIUtility.systemCopyBuffer = sessionCodeText.text.Split(" ")[1])
                .AddTo(gameObject);
        }

        [Inject]
        internal void Initialize(SessionServiceFacade sessionServiceFacade)
        {
            var code = sessionServiceFacade.CurrentSession.Code;
            sessionCodeText.text = $"セッションコード: {code}";
        }
    }
}