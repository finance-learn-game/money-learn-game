using System;
using System.Linq;
using JetBrains.Annotations;
using R3;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.Data;
using SmbcApp.LearnGame.GamePlay.GamePlayObjects.Character;
using SmbcApp.LearnGame.GamePlay.GamePlayObjects.RuntimeDataContainers;
using SmbcApp.LearnGame.GamePlay.GameState.NetworkData;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace SmbcApp.LearnGame.GamePlay.Domain
{
    /// <summary>
    ///     保有株式に関するドメインロジックを提供する
    /// </summary>
    internal sealed class StockDomain : NetworkBehaviour
    {
        [SerializeField] [Required] private PersistantPlayerRuntimeCollection playerCollection;

        [Inject] internal NetworkGameTurn GameTurn;
        [Inject] internal MasterData MasterData;

        [Rpc(SendTo.Server)]
        public void ServerTradeRpc(TradeRequest tradeRequest)
        {
            _ = TryTradeStock(tradeRequest);
        }

        [CanBeNull]
        public Observable<NetworkListEvent<int>> ObserveStockState(ulong clientId)
        {
            return playerCollection.TryGetPlayer(clientId, out var player)
                ? player.StockState.OnChanged
                : null;
        }

        public bool TryGetPlayerStockState(ulong clientId, out NetworkStockState stockState)
        {
            if (playerCollection.TryGetPlayer(clientId, out var player))
            {
                stockState = player.StockState;
                return true;
            }

            stockState = null;
            return false;
        }

        private bool TryTradeStock(TradeRequest request)
        {
            if (!playerCollection.TryGetPlayer(request.ClientId, out var player)) return false;

            var stockState = player.StockState;
            var stockAmounts = stockState.StockAmounts;
            var orgId = request.OrgId;

            if (orgId < 0 || orgId >= stockAmounts.Count) return false;

            var newAmount = stockAmounts[orgId] + request.Amount;
            if (newAmount < 0) return false;

            if (!TryGetStockDataByDate(orgId, GameTurn.CurrentTime, out var stockData)) return false;

            var changeMoney = stockData.StockPrice * -request.Amount;
            var balanceState = player.BalanceState;
            if (balanceState.CurrentBalance + changeMoney < 0) return false;

            balanceState.CurrentBalance += changeMoney;
            stockAmounts[orgId] = newAmount;
            return true;
        }

        /// <summary>
        ///     指定した日付の株式データを取得する
        /// </summary>
        /// <param name="orgId">会社ID</param>
        /// <param name="date">取得する日付</param>
        /// <param name="stockData">取得したデータ</param>
        /// <returns>取得できたか</returns>
        public bool TryGetStockDataByDate(int orgId, DateTime date, out StockData stockData)
        {
            var stockDate = new DateTime(date.Year, date.Month, 1);
            var view = MasterData.DB.StockDataTable.FindClosestByDate(stockDate, false);
            stockData = view.FirstOrDefault(s => s.OrganizationId == orgId);
            return stockData != null;
        }

        /// <summary>
        ///     クライアントプレイヤーがサーバーに株式の売買をリクエストするデータ
        /// </summary>
        public struct TradeRequest : INetworkSerializable
        {
            private ulong _clientId;
            private int _orgId;
            private int _amount;

            public ulong ClientId => _clientId;
            public int OrgId => _orgId;

            /// <summary>
            ///     0以上で購入、0未満で売却
            /// </summary>
            public int Amount => _amount;

            /// <param name="clientId">クライアントID</param>
            /// <param name="orgId">売買する株式の会社ID</param>
            /// <param name="amount">0以上で購入、0未満で売却</param>
            public TradeRequest(ulong clientId, int orgId, int amount)
            {
                _clientId = clientId;
                _orgId = orgId;
                _amount = amount;
            }

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref _clientId);
                serializer.SerializeValue(ref _orgId);
                serializer.SerializeValue(ref _amount);
            }

            public override string ToString()
            {
                return $"TradeRequest {{clientId: {_clientId}, orgId: {_orgId}, amount: {_amount}}}";
            }
        }
    }
}