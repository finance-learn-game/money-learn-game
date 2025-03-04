using SmbcApp.LearnGame.Infrastructure;
using SmbcApp.LearnGame.Utils;

namespace SmbcApp.LearnGame.ConnectionManagement
{
    public struct SessionPlayerData : ISessionPlayerData
    {
        public string PlayerName;
        public int PlayerNumber;
        public NetworkGuid AvatarNetworkGuid;
        public int CurrentBalance;
        public bool HasCharacterSpawned;

        public SessionPlayerData(
            ulong clientID,
            string playerName,
            in NetworkGuid avatarNetworkGuid,
            int currentBalance,
            bool isConnected = false,
            bool hasCharacterSpawned = false
        )
        {
            PlayerName = playerName;
            PlayerNumber = -1;
            AvatarNetworkGuid = avatarNetworkGuid;
            CurrentBalance = currentBalance;
            HasCharacterSpawned = hasCharacterSpawned;
            IsConnected = isConnected;
            ClientID = clientID;
        }

        public bool IsConnected { get; set; }
        public ulong ClientID { get; set; }

        public void Reinitialize()
        {
            HasCharacterSpawned = false;
        }
    }
}