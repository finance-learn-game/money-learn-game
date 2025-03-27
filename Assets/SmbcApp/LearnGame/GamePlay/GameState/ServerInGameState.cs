using MessagePipe;
using R3;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.ApplicationLifecycle.Messages;
using SmbcApp.LearnGame.Data;
using SmbcApp.LearnGame.GamePlay.Configuration;
using SmbcApp.LearnGame.GamePlay.Domain;
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
        [SerializeField] [Required] private AvatarRegistry avatarRegistry;
        [SerializeField] [Required] private PersistantPlayerRuntimeCollection playerCollection;
        [SerializeField] [Required] private TownPartsRegistry townPartsRegistry;

        private SalaryDomain _salaryDomain;

        [Inject] internal IBufferedSubscriber<OnInGameStartMessage> GameStartMessageSubscriber;
        [Inject] internal MasterData MasterData;
        public override GameState ActiveState => GameState.Game;

        protected override void OnServerNetworkSpawn()
        {
            // ゲーム開始時間を設定
            GameStartMessageSubscriber
                .Subscribe(msg => gameTurn.GameRange = (msg.StartDate, msg.EndDate))
                .Dispose();
            gameTurn.CurrentTime = gameTurn.GameRange.Start;

            // 給料ドメインを初期化
            _salaryDomain = new SalaryDomain(avatarRegistry, gameTurn);

            // ターン終了時のイベントを購読
            gameTurn.OnTurnEnd.Subscribe(OnTurnEnd).AddTo(gameObject);
        }

        private void OnTurnEnd(Unit _)
        {
            foreach (var player in playerCollection.Players)
                _salaryDomain.ApplySalary(player.Value);

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