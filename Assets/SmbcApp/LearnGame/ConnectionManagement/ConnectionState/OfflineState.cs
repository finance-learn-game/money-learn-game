using Cysharp.Threading.Tasks;
using SmbcApp.LearnGame.SceneLoader;
using SmbcApp.LearnGame.UnityService.Session;
using VContainer;

namespace SmbcApp.LearnGame.ConnectionManagement.ConnectionState
{
    internal sealed class OfflineState : ConnectionState
    {
        [Inject] internal SceneLoader.SceneLoader SceneLoader;
        [Inject] internal SessionServiceFacade SessionServiceFacade;

        public override async UniTask Enter()
        {
            await SessionServiceFacade.LeaveSessionAsync();
            ConnectionManager.NetworkManager.Shutdown();
            SceneLoader.LoadScene(AppScenes.MainMenu);
        }

        public override UniTask Exit()
        {
            return UniTask.CompletedTask;
        }

        public override void StartClientSession(string playerName)
        {
            var connectionMethod = ConnectionMethodBase.CreateConnectionMethod(
                SessionServiceFacade,
                ConnectionManager,
                playerName
            );
            ConnectionManager.ChangeState(ConnectionManager.ClientConnecting.Configure(connectionMethod));
        }

        public override void StartServerSession(string playerName)
        {
            var connectionMethod = ConnectionMethodBase.CreateConnectionMethod(
                SessionServiceFacade,
                ConnectionManager,
                playerName
            );
            ConnectionManager.ChangeState(ConnectionManager.StartingServer.Configure(connectionMethod));
        }
    }
}