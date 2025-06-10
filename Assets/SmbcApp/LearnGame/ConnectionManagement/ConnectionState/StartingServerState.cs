using System;
using Cysharp.Threading.Tasks;
using SmbcApp.LearnGame.UnityService.Session;
using Unity.Netcode;
using VContainer;

namespace SmbcApp.LearnGame.ConnectionManagement.ConnectionState
{
    internal sealed class StartingServerState : OnlineState
    {
        private ConnectionMethodBase _connectionMethod;
        [Inject] internal SessionServiceFacade SessionServiceFacade;

        public StartingServerState Configure(ConnectionMethodBase connectionMethod)
        {
            _connectionMethod = connectionMethod;
            return this;
        }

        public override UniTask Enter()
        {
            return StartHost();
        }

        public override UniTask Exit()
        {
            return UniTask.CompletedTask;
        }

        public override void OnServerStarted()
        {
            ConnectStatusPublisher.Publish(ConnectStatus.Success);
            ConnectionManager.ChangeState(ConnectionManager.Hosting).Forget();
        }

        public override UniTask ApprovalCheck(
            NetworkManager.ConnectionApprovalRequest request,
            NetworkManager.ConnectionApprovalResponse response
        )
        {
            var clientId = request.ClientNetworkId;
            if (clientId != ConnectionManager.NetworkManager.LocalClientId) return UniTask.CompletedTask;

            response.Approved = true;
            response.CreatePlayerObject = true;

            return UniTask.CompletedTask;
        }

        public override void OnServerStopped()
        {
            StartServerFailed().Forget();
        }

        private async UniTask StartHost()
        {
            try
            {
                if (_connectionMethod == null)
                    throw new Exception("ConnectionMethod is not set.");
                await _connectionMethod.SetupServerConnectionAsync();
            }
            catch (Exception)
            {
                await StartServerFailed();
                throw;
            }
        }

        private async UniTask StartServerFailed()
        {
            ConnectStatusPublisher.Publish(ConnectStatus.StartServerFailed);
            await ConnectionManager.ChangeState(ConnectionManager.Offline);
        }
    }
}