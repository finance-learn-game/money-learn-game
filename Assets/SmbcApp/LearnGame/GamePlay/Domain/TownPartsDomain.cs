using Sirenix.OdinInspector;
using SmbcApp.LearnGame.ConnectionManagement;
using SmbcApp.LearnGame.GamePlay.Configuration;
using SmbcApp.LearnGame.GamePlay.GamePlayObjects.RuntimeDataContainers;
using SmbcApp.LearnGame.Infrastructure;
using Unity.Logging;
using Unity.Netcode;
using UnityEngine;

namespace SmbcApp.LearnGame.GamePlay.Domain
{
    internal sealed class TownPartsDomain : NetworkBehaviour
    {
        [SerializeField] [Required] private TownPartsRegistry townPartsRegistry;
        [SerializeField] [Required] private PersistantPlayerRuntimeCollection playerCollection;

        [Rpc(SendTo.Server)]
        public void PurchaseTownPartRpc(int partId, RpcParams rpcParams = default)
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

            player.BalanceState.CurrentBalance -= townPart.Price;
            player.TownPartsState.TownPartDataList.Add(new TownPartData(
                partId,
                Vector3.zero,
                Quaternion.identity,
                false
            ));
        }

        [Rpc(SendTo.Server)]
        public void PlaceTownPartRpc(NetworkGuid partDataId, Vector3 pos, Quaternion rot, RpcParams rpcParams = default)
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

            player.TownPartsState.TownPartDataList[index] = townPartData.CopyWith(pos, rot, true);
        }
    }
}