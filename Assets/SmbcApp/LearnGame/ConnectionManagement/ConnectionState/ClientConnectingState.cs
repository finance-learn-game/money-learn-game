using System;
using Cysharp.Threading.Tasks;
using SmbcApp.LearnGame.UnityService.Session;
using Unity.Logging;
using UnityEngine;
using VContainer;

namespace SmbcApp.LearnGame.ConnectionManagement.ConnectionState
{
    internal class ClientConnectingState : OnlineState
    {
        private ConnectionMethodBase _connectionMethod;
        private UniTask _joinSessionTask;
        private string _sessionCode;

        [Inject] internal SessionServiceFacade SessionServiceFacade;

        public ClientConnectingState Configure(ConnectionMethodBase connectionMethod, string sessionCode)
        {
            _connectionMethod = connectionMethod;
            _sessionCode = sessionCode;
            return this;
        }

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
            ConnectionManager.ChangeState(ConnectionManager.ClientConnected).Forget();
        }

        public override void OnClientDisconnect(ulong clientId)
        {
            StartingClientFailed();
        }

        private void StartingClientFailed()
        {
            var reason = ConnectionManager.NetworkManager.DisconnectReason;
            Log.Error("Failed to start client: {0}", reason);
            if (string.IsNullOrEmpty(reason))
            {
                ConnectStatusPublisher.Publish(ConnectStatus.StartClientFailed);
            }
            else
            {
                var connectStatus = JsonUtility.FromJson<ConnectStatus>(reason);
                ConnectStatusPublisher.Publish(connectStatus);
            }

            ConnectionManager.ChangeState(ConnectionManager.Offline).Forget();
        }

        private async UniTask ConnectClientAsync()
        {
            try
            {
                if (_connectionMethod == null)
                    throw new Exception("Connection method is not set");
                await _connectionMethod.SetupClientConnectionAsync(_sessionCode);
            }
            catch (Exception e)
            {
                Log.Error(e, "Error connecting client, see following exception");
                StartingClientFailed();
            }
        }
    }
}