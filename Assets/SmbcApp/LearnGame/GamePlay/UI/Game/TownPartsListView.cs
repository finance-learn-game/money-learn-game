using Addler.Runtime.Core.LifetimeBinding;
using Cysharp.Threading.Tasks;
using R3;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.GamePlay.Configuration;
using SmbcApp.LearnGame.GamePlay.Domain;
using SmbcApp.LearnGame.GamePlay.GamePlayObjects.RuntimeDataContainers;
using SmbcApp.LearnGame.UIWidgets.ScrollView;
using Unity.Logging;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace SmbcApp.LearnGame.GamePlay.UI.Game
{
    internal sealed class TownPartsListView : MonoBehaviour
    {
        [SerializeField] [Required] private TownPartsListItemView.Ref itemPrefab;
        [SerializeField] [Required] private UIScrollView scrollView;
        [SerializeField] [Required] private TownPartsRegistry townPartsRegistry;

        [Inject] internal PersistantPlayerRuntimeCollection PlayerCollection;
        [Inject] internal TownPartsDomain TownPartsDomain;

        private void Start()
        {
            InitListView().Forget();
        }

        private async UniTask InitListView()
        {
            var items = await UniTask.WhenAll(townPartsRegistry.TownParts.Select(async (townPart, i) =>
            {
                var item = await itemPrefab.InstantiateAsync().BindTo(gameObject);
                var thumbnail = await townPart.Thumbnail.LoadAssetAsync().BindTo(item.gameObject);
                item.Configure(townPart.Name, townPart.Price, townPart.Point, townPart.Description, thumbnail);
                item.OnBuyButtonClicked
                    .Subscribe(_ => TownPartsDomain.PurchaseTownPartRpc(i))
                    .AddTo(item);
                scrollView.AddItem(item.transform, true);
                return (townPart, item);
            }));

            var clientId = NetworkManager.Singleton.LocalClientId;
            if (!PlayerCollection.Players.TryGetValue(clientId, out var player))
            {
                Log.Error("Player not found");
                return;
            }

            player.BalanceState.OnChangeAsObservable()
                .Subscribe(currentBalance =>
                {
                    foreach (var (townPart, item) in items)
                        item.IsBuyButtonInteractable = currentBalance >= townPart.Price;
                })
                .AddTo(gameObject);
        }
    }
}