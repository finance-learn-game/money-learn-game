using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SmbcApp.LearnGame.ConnectionManagement.ConnectionState
{
    internal class ClientConnectedState : OnlineState
    {
        public override UniTask Enter()
        {
            return UniTask.CompletedTask;
        }

        public override UniTask Exit()
        {
            return UniTask.CompletedTask;
        }

        public override void OnClientDisconnect(ulong _)
        {
            var reason = ConnectionManager.NetworkManager.DisconnectReason;
            if (string.IsNullOrEmpty(reason) || reason == "Disconnected due to host shutting down.")
            {
                ConnectStatusPublisher.Publish(ConnectStatus.Undefined);
            }
            else
            {
                var connectStatus = JsonUtility.FromJson<ConnectStatus>(reason);
                ConnectStatusPublisher.Publish(connectStatus);
            }

            ConnectionManager.ChangeState(ConnectionManager.Offline);
        }
    }
}