using SmbcApp.LearnGame.Infrastructure;
using UnityEngine;

namespace SmbcApp.LearnGame.GamePlay.Configuration
{
    [CreateAssetMenu(fileName = "Avatar", menuName = "Configuration/Avatar")]
    internal sealed class Avatar : GuidScriptableObject
    {
        [SerializeField] private JobClass jobClass;
        [SerializeField] private Sprite avatarImage;

        public JobClass JobClass => jobClass;
        public Sprite AvatarImage => avatarImage;
    }
}