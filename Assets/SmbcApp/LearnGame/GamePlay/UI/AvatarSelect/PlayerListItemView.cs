using System;
using Addler.Runtime.Core.LifetimeBinding;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Avatar = SmbcApp.LearnGame.GamePlay.Configuration.Avatar;

namespace SmbcApp.LearnGame.GamePlay.UI.AvatarSelect
{
    internal sealed class PlayerListItemView : MonoBehaviour
    {
        [SerializeField] private TMP_Text playerNameText;
        [SerializeField] private TMP_Text avatarNameText;
        [SerializeField] private Image playerAvatarImage;
        [SerializeField] private Image readyIconImage;
        [SerializeField] private Sprite waitIcon;
        [SerializeField] private Sprite readyIcon;

        public string PlayerName
        {
            set => playerNameText.text = value;
        }

        public bool IsReady
        {
            set => readyIconImage.sprite = value ? readyIcon : waitIcon;
        }

        public async UniTask SetPlayerAvatar(Avatar avatar)
        {
            var sprite = avatar.AvatarImage;
            if (sprite.IsValid())
                playerAvatarImage.sprite = await sprite.OperationHandle.Task as Sprite;
            else
                playerAvatarImage.sprite = await sprite.LoadAssetAsync().BindTo(gameObject);

            avatarNameText.text = avatar.AvatarName;
        }

        [Serializable]
        public class Ref : ComponentReference<PlayerListItemView>
        {
            public Ref(string guid) : base(guid)
            {
            }
        }
    }
}