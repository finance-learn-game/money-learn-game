using Addler.Runtime.Core.LifetimeBinding;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.GamePlay.Configuration;
using SmbcApp.LearnGame.GamePlay.GamePlayObjects.RuntimeDataContainers;
using SmbcApp.LearnGame.Infrastructure;
using SmbcApp.LearnGame.UIWidgets.ScrollView;
using UnityEngine;
using VContainer;

namespace SmbcApp.LearnGame.GamePlay.UI.Result
{
    internal sealed class ResultListView : MonoBehaviour
    {
        [SerializeField] [Required] private UIScrollView scrollView;
        [SerializeField] [Required] private ResultListItemView.Ref resultListItemPrefab;
        [SerializeField] [Required] private AvatarRegistry avatarRegistry;

        [Inject] internal PersistantPlayerRuntimeCollection PlayerCollection;

        private void Start()
        {
            InitListView().Forget();
        }

        private async UniTask InitListView()
        {
            await UniTask.WhenAll(PlayerCollection.Players.Values.Select(async player =>
            {
                var avatarGuid = player.AvatarGuidState.AvatarGuid.Value.ToGuid();
                if (!avatarRegistry.TryGetAvatar(avatarGuid, out var avatar)) return null;

                var item = await resultListItemPrefab.InstantiateAsync().BindTo(gameObject);
                await item.Configure(
                    avatar.AvatarImage,
                    player.Name.Value,
                    avatar.AvatarName,
                    player.TownPartsState.CurrentPoint,
                    player.BalanceState.CurrentBalance
                );
                scrollView.AddItem(item.transform, true);
                return item;
            }));
        }
    }
}