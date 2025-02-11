using System;
using Cysharp.Threading.Tasks;
using Unity.Logging;
using UnityEngine;

namespace SmbcApp.LearnGame.ConnectionManagement.ConnectionState
{
    internal class ClientConnectingState : OnlineState
    {
        public override async UniTask Enter()
        {
            await ConnectClientAsync();
        }

        public override UniTask Exit()
        {
            return UniTask.CompletedTask;
        }

        public override void OnClientConnected(ulong clientId)
        {
            ConnectStatusPublisher.Publish(ConnectStatus.Success);
            ConnectionManager.ChangeState(ConnectionManager.ClientConnected);
        }

        public override void OnClientDisconnect(ulong clientId)
        {
            StartingClientFailed();
        }

        private void StartingClientFailed()
        {
            var reason = ConnectionManager.NetworkManager.DisconnectReason;
            if (string.IsNullOrEmpty(reason))
            {
                ConnectStatusPublisher.Publish(ConnectStatus.StartClientFailed);
            }
            else
            {
                var connectStatus = JsonUtility.FromJson<ConnectStatus>(reason);
                ConnectStatusPublisher.Publish(connectStatus);
            }

            ConnectionManager.ChangeState(ConnectionManager.Offline);
        }

        private UniTask ConnectClientAsync()
        {
            try
            {
                if (!ConnectionManager.NetworkManager.StartClient()) throw new Exception("Failed to start client");
            }
            catch (Exception e)
            {
                Log.Error(e, "Error connecting client, see following exception");
                StartingClientFailed();
            }

            return UniTask.CompletedTask;
        }
    }
}