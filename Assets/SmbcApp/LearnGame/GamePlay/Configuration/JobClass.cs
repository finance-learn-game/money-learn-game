using System;
using SmbcApp.LearnGame.GamePlay.GamePlayObjects.Avatar;
using UnityEngine;

namespace SmbcApp.LearnGame.GamePlay.Configuration
{
    [CreateAssetMenu(fileName = "AvatarClass", menuName = "Configuration/AvatarClass")]
    internal sealed class JobClass : ScriptableObject
    {
        [SerializeField] private JobType jobType;

        public JobType JobType => jobType;

        [Serializable]
        public class Ref : ComponentReference<JobClass>
        {
            public Ref(string guid) : base(guid)
            {
            }
        }
    }
}