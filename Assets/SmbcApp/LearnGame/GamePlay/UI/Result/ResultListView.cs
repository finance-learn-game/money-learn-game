using Addler.Runtime.Core.LifetimeBinding;
using Cysharp.Threading.Tasks;
using R3;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.GamePlay.Configuration;
using SmbcApp.LearnGame.GamePlay.GamePlayObjects.RuntimeDataContainers;
using SmbcApp.LearnGame.GamePlay.TownBuilding;
using SmbcApp.LearnGame.Infrastructure;
using SmbcApp.LearnGame.UIWidgets.ScrollView;
using UnityEngine;
using UnityScreenNavigator.Runtime.Core.Page;
using VContainer;

namespace SmbcApp.LearnGame.GamePlay.UI.Result
{
    internal sealed class ResultListView : MonoBehaviour
    {
        [SerializeField] [Required] private UIScrollView scrollView;
        [SerializeField] [Required] private ResultListItemView.Ref resultListItemPrefab;
        [SerializeField] [Required] private AvatarRegistry avatarRegistry;
        [SerializeField] [Required] private ResultTownViewPageView.Ref resultTownViewPage;

        [Inject] internal PageContainer PageContainer;
        [Inject] internal PersistantPlayerRuntimeCollection PlayerCollection;
        [Inject] internal ResultTownBuilder ResultTownBuilder;

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
                item.OnViewTownButtonClick.Subscribe(_ =>
                    {
                        ResultTownBuilder.BuildTown(player.TownPartsState.ToEnumerable()).Forget();
                        PageContainer.Push<ResultTownViewPageView>(
                            resultTownViewPage.AssetGUID,
                            true,
                            onLoad: tuple => tuple.page.Configure(player.Name.Value)
                        );
                    }
                ).AddTo(item);
                scrollView.AddItem(item.transform, true);
                return item;
            }));
        }
    }
}