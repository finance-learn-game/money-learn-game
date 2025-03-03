using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using SmbcApp.LearnGame.Data;
using SmbcApp.LearnGame.UIWidgets.Chart;
using Unity.Mathematics;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SmbcApp.LearnGame.Gameplay.GameState
{
    internal sealed class InGameState : GameStateBehaviour
    {
        [SerializeField] private ChartGraphic chartGraphic;

        public override GameState ActiveState => GameState.Game;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<MasterData>().AsSelf();

            builder.RegisterBuildCallback(resolver => UniTask.Void(async () =>
            {
                var masterData = resolver.Resolve<MasterData>();
                await UniTask.WaitUntil(() => masterData.Initialized);

                var data = masterData.DB.StockDataTable.FindRangeByDate(
                    new DateTime(2023, 1, 1),
                    new DateTime(2023, 2, 1)
                );
                chartGraphic.SetData(data.Select(v => (float)v.StockPrice).ToArray(), new float2(500, 3500));
            }));
        }
    }
}