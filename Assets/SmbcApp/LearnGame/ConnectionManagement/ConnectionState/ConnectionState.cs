using Cysharp.Threading.Tasks;
using MessagePipe;
using Unity.Netcode;
using VContainer;

namespace SmbcApp.LearnGame.ConnectionManagement.ConnectionState
{
    internal abstract class ConnectionState
    {
        [Inject] internal ConnectionManager ConnectionManager;
        [Inject] internal IPublisher<ConnectStatus> ConnectStatusPublisher;

        public abstract UniTask Enter();
        public abstract UniTask Exit();

        public virtual void OnClientConnected(ulong clientId)
        {
        }

        public virtual void OnClientDisconnect(ulong clientId)
        {
        }

        public virtual void OnServerStarted()
        {
        }

        public virtual void StartClientSession(string playerName)
        {
        }

        public virtual void StartServerSession(string playerName)
        {
        }

        public virtual void OnUserRequestedShutdown()
        {
        }

        public virtual UniTask ApprovalCheck(
            NetworkManager.ConnectionApprovalRequest request,
            NetworkManager.ConnectionApprovalResponse response
        )
        {
            return UniTask.CompletedTask;
        }

        public virtual void OnTransportFailure()
        {
        }

        public virtual void OnServerStopped()
        {
        }
    }
}