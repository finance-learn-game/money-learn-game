namespace SmbcApp.LearnGame.ConnectionManagement.ConnectionState
{
    internal abstract class OnlineState : ConnectionState
    {
        public override void OnUserRequestedShutdown()
        {
            ConnectStatusPublisher.Publish(ConnectStatus.UserRequestedDisconnect);
            ConnectionManager.ChangeState(ConnectionManager.Offline);
        }

        public override void OnTransportFailure()
        {
            ConnectionManager.ChangeState(ConnectionManager.Offline);
        }
    }
}