using Sirenix.OdinInspector;
using SmbcApp.LearnGame.UnityService.Session;
using TMPro;
using UnityEngine;
using VContainer;

namespace SmbcApp.LearnGame.GamePlay.UI.AvatarSelect
{
    internal sealed class SessionCodeView : MonoBehaviour
    {
        [SerializeField] [Required] private TMP_Text sessionCodeText;

        [Inject]
        internal void Initialize(SessionServiceFacade sessionServiceFacade)
        {
            var code = sessionServiceFacade.CurrentSession.Code;
            sessionCodeText.text = $"セッションコード: {code}";
        }
    }
}