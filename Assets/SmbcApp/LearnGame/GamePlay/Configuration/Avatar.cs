using Sirenix.OdinInspector;
using SmbcApp.LearnGame.Infrastructure;
using UnityEngine;

namespace SmbcApp.LearnGame.GamePlay.Configuration
{
    [CreateAssetMenu(fileName = "Avatar", menuName = "Configuration/Avatar")]
    public sealed class Avatar : GuidScriptableObject
    {
        [SerializeField] [Required] private string avatarName;
        [SerializeField] [Required] private string description;
        [SerializeField] [Required] private JobType jobType;
        [SerializeField] [Required] private Sprite avatarImage;

        public string AvatarName => avatarName;
        public string Description => description;
        public JobType JobType => jobType;
        public Sprite AvatarImage => avatarImage;
    }
}