using SmbcApp.LearnGame.Utils;
using UnityEngine;

namespace SmbcApp.LearnGame.GamePlay.Configuration
{
    [CreateAssetMenu(fileName = "Game Configuration", menuName = "Configuration/Game Configuration", order = 0)]
    [SingletonScriptable("Game Configuration")]
    internal sealed class GameConfiguration : SingletonScriptableObject<GameConfiguration>
    {
        [SerializeField] private int initialBalance = 10000;

        public int InitialBalance => initialBalance;
    }
}