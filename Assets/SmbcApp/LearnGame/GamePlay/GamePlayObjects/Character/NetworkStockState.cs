using R3;
using SmbcApp.LearnGame.Utils;
using Unity.Netcode;
using UnityEngine;

namespace SmbcApp.LearnGame.GamePlay.GamePlayObjects.Character
{
    [DisallowMultipleComponent]
    internal sealed class NetworkStockState : NetworkBehaviour
    {
        public NetworkList<int> StockAmounts;
        public Observable<NetworkListEvent<int>> OnChanged => StockAmounts.OnChangedAsObservable();

        private void Awake()
        {
            StockAmounts = new NetworkList<int>();
        }

        public int[] ToArray()
        {
            var array = new int[StockAmounts.Count];
            for (var i = 0; i < StockAmounts.Count; i++) array[i] = StockAmounts[i];
            return array;
        }

        public void SetStockAmount(int[] amounts)
        {
            StockAmounts.Clear();
            foreach (var amount in amounts) StockAmounts.Add(amount);
        }
    }
}