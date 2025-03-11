using System;
using R3;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.Data;
using SmbcApp.LearnGame.GamePlay.GamePlayObjects.RuntimeDataContainers;
using SmbcApp.LearnGame.GamePlay.GameState.NetworkData;
using SmbcApp.LearnGame.Utils;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace SmbcApp.LearnGame.Gameplay.GameState
{
    [RequireComponent(typeof(NetCodeHooks))]
    internal sealed class ServerInGameState : ServerGameStateBehaviour
    {
        [SerializeField] [Required] private NetworkGameTurn gameTurn;
        [SerializeField] [Required] private PersistantPlayerRuntimeCollection playerCollection;

        [Inject] internal MasterData MasterData;
        public override GameState ActiveState => GameState.Game;

        protected override void OnServerNetworkSpawn()
        {
            // ゲーム開始時間を設定
            var db = MasterData.DB;
            var minDate = db.StockDataTable.SortByDate.First.Date.AddMonths(14);
            gameTurn.CurrentTime = new DateTime(minDate.Year, minDate.Month, 1);

            // ターン終了時のイベントを購読
            gameTurn.OnTurnEnd.Subscribe(OnTurnEnd).AddTo(gameObject);
        }

        private void OnTurnEnd(Unit _)
        {
            gameTurn.CurrentTime = gameTurn.CurrentTime.AddMonths(1);
        }

        protected override void OnClientDisconnect(ulong clientId)
        {
            // ターン終了リストからプレイヤーを削除
            gameTurn.RemovePlayer(clientId);
        }

        protected override void OnSceneEvent(SceneEvent sceneEvent)
        {
            if (sceneEvent.SceneEventType != SceneEventType.LoadComplete) return;
            if (!playerCollection.TryGetPlayer(sceneEvent.ClientId, out var player)) return;

            // 既にデータが設定されている場合は、初期化しない (再接続時や、保存データ読み込み時)
            if (player.StockState.StockAmounts.Count == 0)
                // プレイヤーの初期データを設定
                player.StockState.SetStockAmount(new int[MasterData.DB.OrganizationDataTable.Count]);

            // ターン終了リストにプレイヤーを追加
            gameTurn.AddPlayer(sceneEvent.ClientId);
        }
    }
}