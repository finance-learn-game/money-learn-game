using System;
using R3;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.Data;
using SmbcApp.LearnGame.Utils;
using UnityEngine;
using VContainer;

namespace SmbcApp.LearnGame.Gameplay.GameState
{
    [RequireComponent(typeof(NetCodeHooks))]
    internal sealed class ServerInGameState : GameStateBehaviour
    {
        [SerializeField] [Required] private NetworkGameTime gameTime;
        [SerializeField] [Required] private NetCodeHooks netCodeHooks;

        [Inject] internal MasterData MasterData;
        public override GameState ActiveState => GameState.Game;

        protected override void Awake()
        {
            base.Awake();

            netCodeHooks.OnNetworkSpawnHook.Subscribe(OnNetworkSpawn).AddTo(gameObject);
        }

        private void OnNetworkSpawn(Unit _)
        {
            // ゲーム開始時間を設定
            var db = MasterData.DB;
            var minDate = db.StockDataTable.SortByDate.First.Date.AddMonths(6);
            gameTime.CurrentTime = new DateTime(minDate.Year, minDate.Month, 1);
        }
    }
}