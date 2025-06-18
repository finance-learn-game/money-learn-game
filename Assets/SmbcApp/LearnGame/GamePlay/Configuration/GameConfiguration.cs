using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.Utils;
using UnityEngine;

namespace SmbcApp.LearnGame.GamePlay.Configuration
{
    [CreateAssetMenu(fileName = "Game Configuration", menuName = "Configuration/Game Configuration", order = 0)]
    [SingletonScriptable("Game Configuration")]
    public sealed class GameConfiguration : SingletonScriptableObject<GameConfiguration>
    {
        public enum ConnectionMethodType
        {
            Direct,
            Relay
        }

        [BoxGroup("Network")] [SerializeField] private int maxPlayers = 10;

        [BoxGroup("Network")] [SerializeField]
        private ConnectionMethodType connectionMethodType = ConnectionMethodType.Direct;

        [BoxGroup("Network")] [SerializeField] private bool useWebsockets;

        [SerializeField] private int initialBalance = 10000;
        [SerializeField] private StockChartOption[] stockChartOptions;

        public int MaxPlayers => maxPlayers;
        public int InitialBalance => initialBalance;
        public ConnectionMethodType ConnectionMethod => connectionMethodType;
        public bool UseWebsockets => useWebsockets;
        public IReadOnlyList<StockChartOption> StockChartOptions => stockChartOptions;

        public void SetConnectionMethod(ConnectionMethodType method)
        {
            connectionMethodType = method;
        }

        public void SetUseWebsockets(bool use)
        {
            useWebsockets = use;
        }

        [Serializable]
        public struct StockChartOption
        {
            [SerializeField] private string label;
            [SerializeField] private int months;

            public string Label => label;
            public int Months => months;
        }
    }
}