using Sirenix.OdinInspector;
using SmbcApp.LearnGame.GamePlay.GamePlayObjects.RuntimeDataContainers;
using UnityEngine;
using VContainer;

namespace SmbcApp.LearnGame.Gameplay.GameState
{
    internal sealed class ResultState : GameStateBehaviour
    {
        [SerializeField] [Required] private PersistantPlayerRuntimeCollection playerCollection;

        public override GameState ActiveState => GameState.Result;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(playerCollection);
        }
    }
}