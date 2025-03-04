using Unity.Netcode;

namespace SmbcApp.LearnGame.SceneLoader
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