using System.Collections;
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
            IP,
            Relay
        }

        [BoxGroup("Network")] [SerializeField] private int maxPlayers = 10;

        [ValueDropdown(nameof(_connectionTypes))] [BoxGroup("Network")] [SerializeField]
        private string connectionType = "dtls";

        [BoxGroup("Network")] [SerializeField]
        private ConnectionMethodType connectionMethodType = ConnectionMethodType.IP;

        [SerializeField] private int initialBalance = 10000;

        private IEnumerable _connectionTypes = new ValueDropdownList<string> { "udp", "dtls", "ws", "wss" };

        public int MaxPlayers => maxPlayers;
        public int InitialBalance => initialBalance;
        public string ConnectionType => connectionType;
        public ConnectionMethodType ConnectionMethod => connectionMethodType;
    }
}