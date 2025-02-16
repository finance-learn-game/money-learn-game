using System;
using R3;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.UIWidgets.Button;
using UnityEngine;
using UnityScreenNavigator.Runtime.Core.Modal;
using VContainer;
using ScreenNavigatorModal = UnityScreenNavigator.Runtime.Core.Modal.Modal;

namespace SmbcApp.LearnGame.UIWidgets.Modal
{
    public class UIModal : ScreenNavigatorModal
    {
        [Required] [SerializeField] private UIButton closeButton;

        [Inject] internal ModalContainer ModalContainer;

        protected virtual void Start()
        {
            if (ModalContainer == null)
                ModalContainer = ModalContainer.Find("Main");
            closeButton.OnClick
                .Subscribe(_ => ModalContainer.Pop(true))
                .AddTo(gameObject);
        }

        [Serializable]
        public class Ref : ComponentReference<UIModal>
        {
            public Ref(string guid) : base(guid)
            {
            }
        }
    }
}