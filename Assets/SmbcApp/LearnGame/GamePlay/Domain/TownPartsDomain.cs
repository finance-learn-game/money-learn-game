using System;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.ConnectionManagement;
using SmbcApp.LearnGame.GamePlay.Configuration;
using SmbcApp.LearnGame.GamePlay.GamePlayObjects.RuntimeDataContainers;
using SmbcApp.LearnGame.GamePlay.TownBuilding;
using SmbcApp.LearnGame.Infrastructure;
using Unity.Logging;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace SmbcApp.LearnGame.GamePlay.Domain
{
    internal sealed class TownPartsDomain : NetworkBehaviour
    {
        [SerializeField] [Required] private TownPartsRegistry townPartsRegistry;
        [SerializeField] [Required] private PersistantPlayerRuntimeCollection playerCollection;

        [Inject] internal GridBuildingPlacer BuildingPlacer;

        [Rpc(SendTo.Server)]
        public void PurchaseTownPartRpc(int partId, RpcParams rpcParams = default)
        {
            try
            {
                var clientId = rpcParams.Receive.SenderClientId;
                if (partId < 0 || townPartsRegistry.TownParts.Count < partId)
                {
                    Log.Error("Invalid part id (clientId: {0}, partId: {1})", clientId, partId);
                    return;
                }

                if (!playerCollection.TryGetPlayer(clientId, out var player))
                {
                    Log.Error("Player not found (clientId: {0})", clientId);
                    return;
                }

                var townPart = townPartsRegistry.TownParts[partId];
                if (player.BalanceState.CurrentBalance < townPart.Price)
                {
                    Log.Info("Insufficient balance (clientId: {0}, partId: {1})", clientId, partId);
                    return;
                }

                var partData = new TownPartData(
                    partId,
                    Vector3.zero,
                    false
                );
                player.BalanceState.CurrentBalance -= townPart.Price;
                player.TownPartsState.TownPartDataList.Add(partData);

                // 建物を購入したことをクライアントに通知する
                OnTownPartPurchasedRpc(partId, partData.DataId, RpcTarget.Single(clientId, RpcTargetUse.Temp));
            }
            catch (Exception e)
            {
                Log.Error(e, "Error occurred while purchasing town part (partId: {0}, clientId: {1})", partId,
                    rpcParams.Receive.SenderClientId);
                throw;
            }
        }

        [Rpc(SendTo.Server)]
        private void PlaceTownPartRpc(NetworkGuid partDataId, Vector3 pos, bool isPlaced, RpcParams rpcParams = default)
        {
            var clientId = rpcParams.Receive.SenderClientId;
            if (!playerCollection.TryGetPlayer(clientId, out var player))
            {
                Log.Error("Player not found (clientId: {0})", clientId);
                return;
            }

            if (!player.TownPartsState.TryGetPartData(partDataId, out var townPartData, out var index))
            {
                Log.Error("Town part data not found (clientId: {0}, partDataId: {1})", clientId, partDataId);
                return;
            }

            player.TownPartsState.TownPartDataList[index] = townPartData.CopyWith(pos, isPlaced);
        }

        [Rpc(SendTo.SpecifiedInParams)]
        private void OnTownPartPurchasedRpc(int partId, NetworkGuid partDataGuid, RpcParams rpcParams = default)
        {
            UniTask.Void(async () =>
            {
                var townPart = townPartsRegistry.TownParts[partId];

                // 配置
                var placedPos = await BuildingPlacer.Place(townPart.Prefab);
                if (placedPos == null)
                {
                    Log.Error("[TownPartsDomain] Failed to place building");
                    return;
                }

                // プレイヤーのデータに配置情報を追加
                PlaceTownPartRpc(partDataGuid, placedPos.Value, true);
            });
        }
    }
}