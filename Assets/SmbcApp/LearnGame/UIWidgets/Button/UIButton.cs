using R3;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using uPalette.Generated;
using uPalette.Runtime.Core.Synchronizer.Color;

namespace SmbcApp.LearnGame.UIWidgets.Button
{
    [RequireComponent(typeof(UnityEngine.UI.Button))]
    public sealed class UIButton : MonoBehaviour
    {
        [SerializeField] [Required] private UnityEngine.UI.Button button;
        [SerializeField] [Required] private GraphicColorSynchronizer colorSynchronizer;
        [SerializeField] private GraphicColorSynchronizer textColorSynchronizer;
        [SerializeField] private TMP_Text buttonText;

        private readonly Subject<Unit> _onClick = new();
        public Observable<Unit> OnClick => _onClick;

        public bool IsInteractable
        {
            get => button.interactable;
            set => button.interactable = value;
        }

        public string Text
        {
            get => buttonText.text;
            set => buttonText.text = value;
        }

        private void Start()
        {
            button.onClick.AddListener(OnButtonClick);
        }

        private void OnButtonClick()
        {
            _onClick.OnNext(Unit.Default);
        }

        public void SetColor(ColorEntry entry)
        {
            colorSynchronizer.SetEntryId(entry.ToEntryId());
        }

        public void SetTextColor(ColorEntry entry)
        {
            textColorSynchronizer?.SetEntryId(entry.ToEntryId());
        }
    }
}