using System;
using R3;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.UIWidgets.Button;
using UnityEngine;
using UnityScreenNavigator.Runtime.Core.Modal;

namespace SmbcApp.LearnGame.UIWidgets.Modal
{
    public sealed class UIModal : MonoBehaviour
    {
        [Required] [SerializeField] private UIButton closeButton;
        [SerializeField] private ModalContainer modalContainer;

        private void Start()
        {
            if (modalContainer == null)
                modalContainer = ModalContainer.Find("Main");
            closeButton.OnClick
                .Subscribe(_ => modalContainer.Pop(true))
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