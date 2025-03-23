using Sirenix.OdinInspector;
using SmbcApp.LearnGame.GamePlay.Domain;
using SmbcApp.LearnGame.GamePlay.GamePlayObjects.RuntimeDataContainers;
using SmbcApp.LearnGame.GamePlay.GameState.NetworkData;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SmbcApp.LearnGame.Gameplay.GameState
{
    internal sealed class ClientInGameState : GameStateBehaviour
    {
        [SerializeField] [Required] private NetworkGameTurn gameTurn;
        [SerializeField] [Required] private StockDomain stockDomain;
        [SerializeField] [Required] private TownPartsDomain townPartsDomain;
        [SerializeField] [Required] private PersistantPlayerRuntimeCollection playerCollection;

        public override GameState ActiveState => GameState.Game;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(gameTurn);
            builder.RegisterComponent(stockDomain);
            builder.RegisterComponent(townPartsDomain);
            builder.RegisterInstance(playerCollection);
        }
    }
}