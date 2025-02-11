using Unity.Netcode;

namespace SmbcApp.LearnGame.Utils
{
    public class NetworkedLoadingProgressTracker : NetworkBehaviour
    {
        public NetworkVariable<float> Progress { get; } = new(
            0f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}