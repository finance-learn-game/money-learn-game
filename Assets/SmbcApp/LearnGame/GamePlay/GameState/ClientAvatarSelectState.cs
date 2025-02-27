using Sirenix.OdinInspector;
using SmbcApp.LearnGame.Utils;
using UnityEngine;

namespace SmbcApp.LearnGame.Gameplay.GameState
{
    [RequireComponent(typeof(NetCodeHooks))]
    internal sealed class ClientAvatarSelectState : GameStateBehaviour
    {
        [SerializeField] [Required] private NetCodeHooks netCodeHooks;
        [SerializeField] [Required] private NetworkAvatarSelection networkAvatarSelection;
        public override GameState ActiveState => GameState.CharSelect;


        public void OnPlayerClickedReady()
        {
        }
    }
}