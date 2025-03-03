using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SmbcApp.LearnGame.Gameplay.GameState
{
    internal sealed class ClientInGameState : GameStateBehaviour
    {
        [SerializeField] [Required] private NetworkGameTime gameTime;

        public override GameState ActiveState => GameState.Game;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(gameTime);
        }
    }
}