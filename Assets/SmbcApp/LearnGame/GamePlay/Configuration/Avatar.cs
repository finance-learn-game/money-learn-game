using System;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.Infrastructure;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SmbcApp.LearnGame.GamePlay.Configuration
{
    [CreateAssetMenu(fileName = "Avatar", menuName = "Configuration/Avatar")]
    public sealed class Avatar : GuidScriptableObject
    {
        [SerializeField] [Required] private string avatarName;
        [SerializeField] [Required] private string description;
        [SerializeField] [Required] private JobType jobType;
        [SerializeField] [Required] private AssetReferenceSprite avatarImage;
        [SerializeField] [Required] private SalaryConfig salaryConfig;

        public string AvatarName => avatarName;
        public string Description => description;
        public JobType JobType => jobType;
        public AssetReferenceSprite AvatarImage => avatarImage;
        public SalaryConfig SalaryConfig => salaryConfig;

        [Serializable]
        public class Ref : AssetReferenceT<Avatar>
        {
            public Ref(string guid) : base(guid)
            {
            }
        }
    }
}