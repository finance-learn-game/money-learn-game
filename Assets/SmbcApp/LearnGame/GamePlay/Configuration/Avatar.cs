using System;
using SmbcApp.LearnGame.Utils;
using UnityEngine;

namespace SmbcApp.LearnGame.GamePlay.Configuration
{
    [CreateAssetMenu(fileName = "Avatar", menuName = "Configuration/Avatar")]
    internal sealed class Avatar : ScriptableObject
    {
        [SerializeField] private JobClass.Ref jobClass;
        [SerializeField] private SpriteReference avatarImage;

        public JobClass.Ref JobClass => jobClass;
        public SpriteReference AvatarImage => avatarImage;

        [Serializable]
        public class Ref : ComponentReference<Avatar>
        {
            public Ref(string guid) : base(guid)
            {
            }
        }
    }
}