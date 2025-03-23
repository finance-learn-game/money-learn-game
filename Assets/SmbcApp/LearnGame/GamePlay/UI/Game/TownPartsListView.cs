using Addler.Runtime.Core.LifetimeBinding;
using Cysharp.Threading.Tasks;
using R3;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.GamePlay.Configuration;
using SmbcApp.LearnGame.GamePlay.Domain;
using SmbcApp.LearnGame.UIWidgets.ScrollView;
using UnityEngine;
using VContainer;

namespace SmbcApp.LearnGame.GamePlay.UI.Game
{
    internal sealed class TownPartsListView : MonoBehaviour
    {
        [SerializeField] [Required] private TownPartsListItemView.Ref itemPrefab;
        [SerializeField] [Required] private UIScrollView scrollView;
        [SerializeField] [Required] private TownPartsRegistry townPartsRegistry;

        [Inject] internal TownPartsDomain TownPartsDomain;

        private void Start()
        {
            InitListView().Forget();
        }

        private async UniTask InitListView()
        {
            await UniTask.WhenAll(townPartsRegistry.TownParts.Select(async (townPart, i) =>
            {
                var item = await itemPrefab.InstantiateAsync().BindTo(gameObject);
                var thumbnail = await townPart.Thumbnail.LoadAssetAsync().BindTo(item.gameObject);
                item.Configure(townPart.Name, townPart.Price, townPart.Point, townPart.Description, thumbnail);
                item.OnBuyButtonClicked
                    .Subscribe(_ => TownPartsDomain.PurchaseTownPartRpc(i))
                    .AddTo(item);
                scrollView.AddItem(item.transform, true);
            }));
        }
    }
}