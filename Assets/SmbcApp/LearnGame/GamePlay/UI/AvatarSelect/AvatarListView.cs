using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.GamePlay.Configuration;
using SmbcApp.LearnGame.UIWidgets.ScrollView;
using UnityEngine;
using Avatar = SmbcApp.LearnGame.GamePlay.Configuration.Avatar;

namespace SmbcApp.LearnGame.GamePlay.UI.AvatarSelect
{
    internal sealed class AvatarListView : MonoBehaviour
    {
        [SerializeField] [Required] private UIScrollView scrollView;
        [SerializeField] [Required] private AvatarListItemView.Ref avatarListItemViewPrefab;
        [SerializeField] [Required] private AvatarRegistry avatarRegistry;

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

            scrollView.AddItem(item.transform, true);
        }
    }
}