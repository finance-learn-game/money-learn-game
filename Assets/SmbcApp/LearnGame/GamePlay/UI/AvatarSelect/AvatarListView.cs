using Cysharp.Threading.Tasks;
using R3;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.GamePlay.Configuration;
using SmbcApp.LearnGame.Gameplay.GameState;
using SmbcApp.LearnGame.Infrastructure;
using SmbcApp.LearnGame.UIWidgets.ScrollView;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using Avatar = SmbcApp.LearnGame.GamePlay.Configuration.Avatar;

namespace SmbcApp.LearnGame.GamePlay.UI.AvatarSelect
{
    internal sealed class AvatarListView : MonoBehaviour
    {
        [SerializeField] [Required] private UIScrollView scrollView;
        [SerializeField] [Required] private AvatarListItemView.Ref avatarListItemViewPrefab;
        [SerializeField] [Required] private AvatarRegistry avatarRegistry;

        [Inject] internal NetworkAvatarSelection NetworkAvatarSelection;

        private void Start()
        {
            foreach (var avatar in avatarRegistry.Avatars)
                AddAvatarToListView(avatar).Forget();
        }

        private async UniTask AddAvatarToListView(Avatar avatar)
        {
            var itemGo = await avatarListItemViewPrefab.InstantiateAsync();
            itemGo.TryGetComponent(out AvatarListItemView item);
            item.Configure(avatar);

            item.OnSelect.Subscribe(_ =>
            {
                var networkManager = NetworkManager.Singleton;
                NetworkAvatarSelection.ServerChangeStateRpc(
                    networkManager.LocalClientId,
                    false,
                    avatar.Guid.ToNetworkGuid()
                );
            }).AddTo(item);

            scrollView.AddItem(item.transform, true);
        }
    }
}