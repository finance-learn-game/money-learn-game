using System;
using R3;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.UIWidgets.Button;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SmbcApp.LearnGame.GamePlay.UI.Game
{
    internal sealed class TownPartsListItemView : MonoBehaviour
    {
        [SerializeField] [Required] private Image thumbnailImage;
        [SerializeField] [Required] private TMP_Text titleText;
        [SerializeField] [Required] private TMP_Text descriptionText;
        [SerializeField] [Required] private UIButton buyButton;

        public Observable<Unit> OnBuyButtonClicked => buyButton.OnClick;

        public bool IsBuyButtonInteractable
        {
            get => buyButton.IsInteractable;
            set => buyButton.IsInteractable = value;
        }

        public void Configure(string title, int price, int point, string description, Sprite thumbnail)
        {
            titleText.text = $"{title} ￥{price} ({point} pt)";
            descriptionText.text = description;
            thumbnailImage.sprite = thumbnail;
        }

        [Serializable]
        public class Ref : ComponentReference<TownPartsListItemView>
        {
            public Ref(string guid) : base(guid)
            {
            }
        }
    }
}