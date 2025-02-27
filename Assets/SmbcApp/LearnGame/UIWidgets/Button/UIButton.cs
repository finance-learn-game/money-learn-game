using R3;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SmbcApp.LearnGame.UIWidgets.Button
{
    [RequireComponent(typeof(UnityEngine.UI.Button))]
    public sealed class UIButton : MonoBehaviour
    {
        [SerializeField] [Required] private UnityEngine.UI.Button button;

        private readonly Subject<Unit> _onClick = new();
        public Observable<Unit> OnClick => _onClick;

        public bool IsInteractable
        {
            get => button.interactable;
            set => button.interactable = value;
        }

        private void Start()
        {
            button.onClick.AddListener(OnButtonClick);
        }

        private void OnButtonClick()
        {
            _onClick.OnNext(Unit.Default);
        }
    }
}