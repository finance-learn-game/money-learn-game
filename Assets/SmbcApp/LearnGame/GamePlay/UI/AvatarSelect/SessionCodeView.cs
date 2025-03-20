using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.UIWidgets.Button;
using SmbcApp.LearnGame.UnityService.Session;
using TMPro;
using Unity.Logging;
using UnityEngine;
using VContainer;

namespace SmbcApp.LearnGame.GamePlay.UI.AvatarSelect
{
    internal sealed class SessionCodeView : MonoBehaviour
    {
        [SerializeField] [Required] private TMP_Text sessionCodeText;
        [SerializeField] [Required] private UIButton copyButton;

        [Inject] internal SessionServiceFacade SessionServiceFacade;

        private void Start()
        {
            InitViewAsync(destroyCancellationToken).Forget();
        }

        private async UniTask InitViewAsync(CancellationToken cancellation = new())
        {
            Log.Info("Waiting for session to be created");
            await UniTask.WaitUntil(() => SessionServiceFacade.CurrentSession != null, cancellationToken: cancellation);
            var code = SessionServiceFacade.CurrentSession.Code;
            sessionCodeText.text = $"セッションコード: {code}";
            Log.Info("SessionCodeView initialized with code {0}", code);
            copyButton.OnClick.Subscribe(_ => GUIUtility.systemCopyBuffer = sessionCodeText.text.Split(" ")[1])
                .AddTo(gameObject);
        }
    }
}