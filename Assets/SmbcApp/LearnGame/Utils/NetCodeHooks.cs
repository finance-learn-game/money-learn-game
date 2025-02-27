using R3;
using Unity.Netcode;

namespace SmbcApp.LearnGame.Utils
{
    public sealed class NetCodeHooks : NetworkBehaviour
    {
        private readonly Subject<Unit> _onNetworkDespawnHook = new();
        private readonly Subject<Unit> _onNetworkSpawnHook = new();

        public Observable<Unit> OnNetworkSpawnHook => _onNetworkSpawnHook;
        public Observable<Unit> OnNetworkDespawnHook => _onNetworkDespawnHook;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _onNetworkSpawnHook.OnNext(Unit.Default);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            _onNetworkDespawnHook.OnNext(Unit.Default);
        }
    }
}