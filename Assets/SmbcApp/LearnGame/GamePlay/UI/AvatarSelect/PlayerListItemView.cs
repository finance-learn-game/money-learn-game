using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SmbcApp.LearnGame.GamePlay.UI.AvatarSelect
{
    internal sealed class PlayerListItemView : MonoBehaviour
    {
        [SerializeField] private TMP_Text playerNameText;
        [SerializeField] private Image playerAvatarImage;
        [SerializeField] private Image readyIconImage;
        [SerializeField] private Sprite waitIcon;
        [SerializeField] private Sprite readyIcon;

        public string PlayerName
        {
            set => playerNameText.text = value;
        }

        public Sprite PlayerAvatar
        {
            set => playerAvatarImage.sprite = value;
        }

        public bool IsReady
        {
            set => readyIconImage.sprite = value ? readyIcon : waitIcon;
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