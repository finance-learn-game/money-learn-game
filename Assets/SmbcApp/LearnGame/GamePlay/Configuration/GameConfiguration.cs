using Sirenix.OdinInspector;
using SmbcApp.LearnGame.Utils;
using UnityEngine;

namespace SmbcApp.LearnGame.GamePlay.Configuration
{
    [CreateAssetMenu(fileName = "Game Configuration", menuName = "Configuration/Game Configuration", order = 0)]
    [SingletonScriptable("Game Configuration")]
    public sealed class GameConfiguration : SingletonScriptableObject<GameConfiguration>
    {
        [BoxGroup("Network")] [SerializeField] private int maxPlayers = 10;
        [SerializeField] private int initialBalance = 10000;

        public int MaxPlayers => maxPlayers;
        public int InitialBalance => initialBalance;
    }
}