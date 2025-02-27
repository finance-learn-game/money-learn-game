using Sirenix.OdinInspector;
using UnityEngine;
using UnityScreenNavigator.Runtime.Core.Page;
using VContainer;

namespace SmbcApp.LearnGame.GamePlay.UI.AvatarSelect
{
    internal sealed class AvatarSelectPageView : Page
    {
        [SerializeField] [Required] private SessionCodeView sessionCodeView;
        [SerializeField] [Required] private PlayerListView playerListView;

        [Inject] internal IObjectResolver Resolver;

        private void Start()
        {
            Resolver.Inject(sessionCodeView);
            Resolver.Inject(playerListView);
        }
    }
}