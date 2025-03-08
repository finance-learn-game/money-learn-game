using Sirenix.OdinInspector;
using SmbcApp.LearnGame.GamePlay.Domain;
using SmbcApp.LearnGame.GamePlay.GameState.NetworkData;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SmbcApp.LearnGame.Gameplay.GameState
{
    internal sealed class ClientInGameState : GameStateBehaviour
    {
        [SerializeField] [Required] private NetworkGameTime gameTime;
        [SerializeField] [Required] private StockDomain stockDomain;

        public override GameState ActiveState => GameState.Game;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(gameTime);
            builder.RegisterComponent(stockDomain);
        }
    }
}