using System;
using R3;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.UIWidgets.Button;
using TMPro;
using UnityEngine;

namespace SmbcApp.LearnGame.Gameplay.UI.MainMenu
{
    public class ProfileListItemView : MonoBehaviour
    {
        [SerializeField] [Required] private TMP_Text profileName;
        [SerializeField] [Required] private UIButton selectButton;
        [SerializeField] [Required] private UIButton deleteButton;

        public Observable<Unit> OnSelect => selectButton.OnClick;
        public Observable<Unit> OnDelete => deleteButton.OnClick;

        public string ProfileName
        {
            get => profileName.text;
            set => profileName.text = value;
        }

        public bool IsSelected
        {
            get => !selectButton.IsInteractable;
            set => selectButton.IsInteractable = !value;
        }

        [Serializable]
        public class Ref : ComponentReference<ProfileListItemView>
        {
            public Ref(string guid) : base(guid)
            {
            }
        }
    }
}