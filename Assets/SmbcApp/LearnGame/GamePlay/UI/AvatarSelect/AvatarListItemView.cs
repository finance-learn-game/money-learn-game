using System;
using Addler.Runtime.Core.LifetimeBinding;
using Cysharp.Threading.Tasks;
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

        public async UniTask Configure(Avatar avatar)
        {
            if (avatar.AvatarImage.IsValid())
                avatarImage.sprite = await avatar.AvatarImage.OperationHandle.Task as Sprite;
            else
                avatarImage.sprite = await avatar.AvatarImage.LoadAssetAsync().BindTo(gameObject);

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