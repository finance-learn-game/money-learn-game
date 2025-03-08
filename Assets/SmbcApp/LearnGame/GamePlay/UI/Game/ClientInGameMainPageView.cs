using R3;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.UIWidgets.Button;
using SmbcApp.LearnGame.UIWidgets.Modal;
using UnityEngine;
using UnityScreenNavigator.Runtime.Core.Modal;
using UnityScreenNavigator.Runtime.Core.Page;
using VContainer;

namespace SmbcApp.LearnGame.GamePlay.UI.Game
{
    internal sealed class ClientInGameMainPageView : Page
    {
        [SerializeField] [Required] private GameTimeTextView timeTextView;
        [SerializeField] [Required] private UIButton openStockModalButton;
        [SerializeField] [Required] private UIModal.Ref stockModalPrefab;

        private ModalContainer _modalContainer;

        private void Start()
        {
            openStockModalButton.OnClick
                .Subscribe(_ => _modalContainer.Push(stockModalPrefab.AssetGUID, true))
                .AddTo(gameObject);
        }

        [Inject]
        internal void Construct(IObjectResolver resolver, ModalContainer modalContainer)
        {
            _modalContainer = modalContainer;

            resolver.Inject(timeTextView);
        }
    }
}