using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;
using UnityScreenNavigator.Runtime.Core.Modal;
using UnityScreenNavigator.Runtime.Core.Page;
using VContainer;
using VContainer.Unity;

namespace SmbcApp.LearnGame.Gameplay.UI.Common
{
    public class UIEntryPoint : LifetimeScope
    {
        [SerializeField] [Required] private PageRef initialPage;
        [SerializeField] private bool enableServerInitialPage;

        [SerializeField] [ShowIf(nameof(enableServerInitialPage))] [Required]
        private PageRef serverInitialPage;

        [SerializeField] [Required] private ModalContainer modalContainer;
        [SerializeField] [Required] private PageContainer pageContainer;

        private ModalCallbackReceiver _modalCallbackReceiver;
        private PageCallbackReceiver _pageCallbackReceiver;

        private void Start()
        {
            _modalCallbackReceiver = new ModalCallbackReceiver(Container);
            _pageCallbackReceiver = new PageCallbackReceiver(Container);

            pageContainer.AddCallbackReceiver(_pageCallbackReceiver);
            modalContainer.AddCallbackReceiver(_modalCallbackReceiver);

#if UNITY_EDITOR
            for (var i = 0; i < pageContainer.transform.childCount; i++)
            {
                var child = pageContainer.transform.GetChild(i);
                child.gameObject.SetActive(false);
            }

            for (var i = 0; i < modalContainer.transform.childCount; i++)
            {
                var child = modalContainer.transform.GetChild(i);
                child.gameObject.SetActive(false);
            }
#endif

            if (enableServerInitialPage && NetworkManager.Singleton.IsServer)
                pageContainer.Push(serverInitialPage.AssetGUID, false);
            else
                pageContainer.Push(initialPage.AssetGUID, false);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            pageContainer.RemoveCallbackReceiver(_pageCallbackReceiver);
            modalContainer.RemoveCallbackReceiver(_modalCallbackReceiver);
        }

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(pageContainer);
            builder.RegisterComponent(modalContainer);
        }

        private sealed class ModalCallbackReceiver : IModalContainerCallbackReceiver
        {
            private readonly IObjectResolver _resolver;

            public ModalCallbackReceiver(IObjectResolver resolver)
            {
                _resolver = resolver;
            }

            public void BeforePush(Modal enterModal, Modal exitModal)
            {
                _resolver.Inject(enterModal);
            }

            public void AfterPush(Modal enterModal, Modal exitModal)
            {
            }

            public void BeforePop(Modal enterModal, Modal exitModal)
            {
            }

            public void AfterPop(Modal enterModal, Modal exitModal)
            {
            }
        }

        private sealed class PageCallbackReceiver : IPageContainerCallbackReceiver
        {
            private readonly IObjectResolver _resolver;

            public PageCallbackReceiver(IObjectResolver resolver)
            {
                _resolver = resolver;
            }

            public void BeforePush(Page enterPage, Page exitPage)
            {
                _resolver.Inject(enterPage);
            }

            public void AfterPush(Page enterPage, Page exitPage)
            {
            }

            public void BeforePop(Page enterPage, Page exitPage)
            {
            }

            public void AfterPop(Page enterPage, Page exitPage)
            {
            }
        }
    }
}