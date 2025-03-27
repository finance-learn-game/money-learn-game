using R3;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.UIWidgets.Button;
using Unity.Netcode;
using UnityEngine;
using UnityScreenNavigator.Runtime.Core.Page;
using VContainer;

namespace SmbcApp.LearnGame.GamePlay.UI.Result
{
    internal sealed class ResultPageView : Page
    {
        [SerializeField] [Required] private ResultListView resultList;
        [SerializeField] [Required] private UIButton exitButton;

        private void Start()
        {
            exitButton.OnClick.Subscribe(_ =>
            {
                var networkManager = NetworkManager.Singleton;
                if (networkManager.ShutdownInProgress) return;

                networkManager.Shutdown();
                exitButton.IsInteractable = false;
            }).AddTo(gameObject);
        }

        [Inject]
        internal void Construct(IObjectResolver resolver)
        {
            resolver.Inject(resultList);
        }
    }
}