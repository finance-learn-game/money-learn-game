using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using R3;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.Data;
using SmbcApp.LearnGame.GamePlay.Domain;
using SmbcApp.LearnGame.GamePlay.GamePlayObjects.Character;
using SmbcApp.LearnGame.GamePlay.GameState.NetworkData;
using SmbcApp.LearnGame.UIWidgets.ScrollView;
using Unity.Logging;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace SmbcApp.LearnGame.GamePlay.UI.Game
{
    internal sealed class StockListView : MonoBehaviour
    {
        [SerializeField] [Required] private UIScrollView stockList;
        [SerializeField] [Required] private StockListItemView.Ref stockListItemPrefab;

        private readonly List<StockListItemView> _stockItems = new();

        [Inject] internal MasterData MasterData;
        [Inject] internal NetworkGameTurn NetworkGameTurn;
        [Inject] internal StockDomain StockDomain;

        private void Start()
        {
            InitStockList().Forget();

            var clientId = NetworkManager.Singleton.LocalClientId;
            StockDomain.ObserveStockState(clientId)?
                .Subscribe(_ => UpdateStockList())
                .AddTo(gameObject);
        }

        private async UniTask InitStockList()
        {
            await UniTask.WaitUntil(() => MasterData.Initialized);

            var organizations = MasterData.DB.OrganizationDataTable.All;
            var clientId = NetworkManager.Singleton.LocalClientId;
            await UniTask.WhenAll(organizations.Select(async org =>
            {
                var item = await stockListItemPrefab.InstantiateAsync();
                stockList.AddItem(item.transform, true);
                _stockItems.Add(item);
                item.OnBuyButtonClicked.Subscribe(_ => StockDomain.ServerTradeRpc(
                    new StockDomain.TradeRequest(
                        clientId,
                        org.Id,
                        1
                    )
                )).AddTo(item.gameObject);
                item.OnSellButtonClicked.Subscribe(_ => StockDomain.ServerTradeRpc(
                    new StockDomain.TradeRequest(
                        clientId,
                        org.Id,
                        -1
                    )
                )).AddTo(item.gameObject);
            }));

            UpdateStockList();
        }

        private void UpdateStockList()
        {
            var clientId = NetworkManager.Singleton.LocalClientId;
            if (!StockDomain.TryGetPlayerStockState(clientId, out var stockData))
            {
                Log.Error("PlayerStockData not found. clientId: {0}", clientId);
                return;
            }

            foreach (var (item, i) in _stockItems.Select((item, i) => (item, i)))
            {
                var org = MasterData.DB.OrganizationDataTable.FindById(i);
                ConfigureItem(NetworkGameTurn.CurrentTime, org, stockData, item);
            }
        }

        private void ConfigureItem(
            DateTime date,
            OrganizationData org,
            NetworkStockState stockState,
            StockListItemView item
        )
        {
            if (StockDomain.TryGetStockDataByDate(org.Id, date, out var stockData))
            {
                var stockAmount = stockState.StockAmounts[org.Id];
                item.Configure(org.Name, stockData.StockPrice, stockAmount);
            }
            else
            {
                Log.Error("StockData not found. date: {0}, orgId: {1}", date, org.Id);
            }
        }
    }
}