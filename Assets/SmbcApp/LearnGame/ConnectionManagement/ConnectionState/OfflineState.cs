using Cysharp.Threading.Tasks;
using SmbcApp.LearnGame.UnityService.Session;
using SmbcApp.LearnGame.Utils;
using VContainer;

namespace SmbcApp.LearnGame.ConnectionManagement.ConnectionState
{
    internal sealed class OfflineState : ConnectionState
    {
        [Inject] internal SceneLoader SceneLoader;
        [Inject] internal SessionServiceFacade SessionServiceFacade;

        public override UniTask Enter()
        {
            ConnectionManager.NetworkManager.Shutdown();
            // TODO: MainMenuシーンをロードする
            return UniTask.CompletedTask;
        }

        public override UniTask Exit()
        {
            return UniTask.CompletedTask;
        }

        public override void StartClientSession(string playerName)
        {
            ConnectionManager.ChangeState(ConnectionManager.ClientConnecting);
        }

        public override void StartServerSession(string playerName)
        {
            var connectionMethod =
                new ConnectionMethodMultiPlaySDK(SessionServiceFacade, ConnectionManager, playerName);
            ConnectionManager.ChangeState(ConnectionManager.StartingServer.Configure(connectionMethod));
        }
    }
}