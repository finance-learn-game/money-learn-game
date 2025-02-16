using Sirenix.OdinInspector;
using SmbcApp.LearnGame.Gameplay.UI.MainMenu;
using Unity.Logging;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SmbcApp.LearnGame.Gameplay.GameState
{
    internal sealed class ClientMainMenuState : GameStateBehaviour
    {
        [SerializeField] [Required] private MainMenuUIMediator mainMenuUIMediator;

        public override GameState ActiveState => GameState.MainMenu;

        protected override void Awake()
        {
            base.Awake();

            if (!string.IsNullOrEmpty(Application.cloudProjectId)) return;
            Log.Error("Cloud Project ID is not set. Please set it in the Unity Services window.");
        }

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);
            builder.RegisterComponent(mainMenuUIMediator);
        }
    }
}