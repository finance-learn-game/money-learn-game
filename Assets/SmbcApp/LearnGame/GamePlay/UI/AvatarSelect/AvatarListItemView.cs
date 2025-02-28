using System;
using R3;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.UIWidgets.Button;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Avatar = SmbcApp.LearnGame.GamePlay.Configuration.Avatar;

namespace SmbcApp.LearnGame.GamePlay.UI.AvatarSelect
{
    internal sealed class AvatarListItemView : MonoBehaviour
    {
        [SerializeField] [Required] private Image avatarImage;
        [SerializeField] [Required] private TMP_Text avatarNameText;
        [SerializeField] [Required] private TMP_Text descriptionText;
        [SerializeField] [Required] private UIButton selectButton;

        public Observable<Unit> OnSelect => selectButton.OnClick;

        public bool Interactable
        {
            get => selectButton.IsInteractable;
            set => selectButton.IsInteractable = value;
        }

        public void Configure(Avatar avatar)
        {
            avatarImage.sprite = avatar.AvatarImage;
            avatarNameText.text = avatar.AvatarName;
            descriptionText.text = avatar.Description;
        }

        [Serializable]
        public class Ref : ComponentReference<AvatarListItemView>
        {
            public Ref(string guid) : base(guid)
            {
            }
        }
    }
}