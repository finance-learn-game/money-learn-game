using Sirenix.OdinInspector;
using SmbcApp.LearnGame.GamePlay.GameState.NetworkData;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SmbcApp.LearnGame.Gameplay.GameState
{
    internal sealed class ClientAvatarSelectState : GameStateBehaviour
    {
        [SerializeField] [Required] private NetworkAvatarSelection networkAvatarSelection;

        public override GameState ActiveState => GameState.AvatarSelect;

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            builder.RegisterComponent(networkAvatarSelection);
        }
    }
}