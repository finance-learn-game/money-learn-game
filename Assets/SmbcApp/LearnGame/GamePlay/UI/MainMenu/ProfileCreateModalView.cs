using R3;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.UIWidgets.Button;
using SmbcApp.LearnGame.UIWidgets.Modal;
using SmbcApp.LearnGame.Utils;
using TMPro;
using UnityEngine;
using VContainer;

namespace SmbcApp.LearnGame.Gameplay.UI.MainMenu
{
    public class ProfileCreateModalView : UIModal
    {
        [SerializeField] [Required] private UIButton createButton;
        [SerializeField] [Required] private TMP_InputField profileNameInput;

        [Inject] internal ProfileManager ProfileManager;

        protected override void Start()
        {
            base.Start();

            createButton.OnClick.Subscribe(_ =>
            {
                var profileName = profileNameInput.text;
                if (string.IsNullOrEmpty(profileName)) return;
                ProfileManager.CreateProfile(profileName);
                ModalContainer.Pop(true);
            }).AddTo(gameObject);
        }
    }
}