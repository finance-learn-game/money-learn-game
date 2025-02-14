using SmbcApp.LearnGame.GamePlay.GamePlayObjects.RuntimeDataContainers;
using SmbcApp.LearnGame.Utils;
using UnityEngine;

namespace SmbcApp.LearnGame.Gameplay.UI.Common
{
    internal sealed class ClientGameLoadingScreen : ClientLoadingScreen
    {
        [SerializeField] private PersistantPlayerRuntimeCollection runtimeCollection;

        protected override void AddOtherPlayerProgressBar(
            ulong clientId,
            NetworkedLoadingProgressTracker progressTracker
        )
        {
            base.AddOtherPlayerProgressBar(clientId, progressTracker);
            LoadingProgressBars[clientId].NameText = GetPlayerName(clientId);
        }

        protected override void UpdateOtherPlayerProgressBar(ulong clientId, int progressBarIndex)
        {
            base.UpdateOtherPlayerProgressBar(clientId, progressBarIndex);
            LoadingProgressBars[clientId].NameText = GetPlayerName(clientId);
        }

        private string GetPlayerName(ulong clientId)
        {
            foreach (var (_, player) in runtimeCollection.Players)
                if (clientId == player.OwnerClientId)
                    return player.Name.Value;

            return "";
        }
    }
}