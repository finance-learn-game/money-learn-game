using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace SmbcApp.LearnGame.GamePlay.UI.Game
{
    internal sealed class NewsListItemView : MonoBehaviour
    {
        [SerializeField] [Required] private TMP_Text dateText;
        [SerializeField] [Required] private TMP_Text contentText;

        public string DateText
        {
            get => dateText.text;
            set => dateText.text = value;
        }

        public string ContentText
        {
            get => contentText.text;
            set => contentText.text = value;
        }

        [Serializable]
        public class Ref : ComponentReference<NewsListItemView>
        {
            public Ref(string guid) : base(guid)
            {
            }
        }
    }
}