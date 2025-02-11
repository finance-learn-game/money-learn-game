using UnityEngine;
using UnityScreenNavigator.Runtime.Core.Modal;
using UnityScreenNavigator.Runtime.Core.Page;
using VContainer;
using VContainer.Unity;

namespace SmbcApp.LearnGame.Gameplay.UI.Common
{
    public class UIEntryPoint : LifetimeScope
    {
        [SerializeField] private PageRef initialPage;

        private ModalCallbackReceiver _modalCallbackReceiver;
        private ModalContainer _modalContainer;
        private PageCallbackReceiver _pageCallbackReceiver;
        private PageContainer _pageContainer;

        private void Start()
        {
            _modalCallbackReceiver = new ModalCallbackReceiver(Container);
            _pageCallbackReceiver = new PageCallbackReceiver(Container);

            _pageContainer.AddCallbackReceiver(_pageCallbackReceiver);
            _modalContainer.AddCallbackReceiver(_modalCallbackReceiver);

#if UNITY_EDITOR
            for (var i = 0; i < _pageContainer.transform.childCount; i++)
            {
                var child = _pageContainer.transform.GetChild(i);
                child.gameObject.SetActive(false);
            }

            for (var i = 0; i < _modalContainer.transform.childCount; i++)
            {
                var child = _modalContainer.transform.GetChild(i);
                child.gameObject.SetActive(false);
            }
#endif

            if (initialPage != null) _pageContainer.Push(initialPage.AssetGUID, false);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _pageContainer.RemoveCallbackReceiver(_pageCallbackReceiver);
            _modalContainer.RemoveCallbackReceiver(_modalCallbackReceiver);
        }

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_pageContainer = PageContainer.Find("Main"));
            builder.RegisterComponent(_modalContainer = ModalContainer.Find("Main"));
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