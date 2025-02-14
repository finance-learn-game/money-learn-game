using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SmbcApp.LearnGame.GamePlay.Configuration
{
    [CreateAssetMenu(fileName = "Avatar Registry", menuName = "Configuration/Avatar Registry", order = 0)]
    internal sealed class AvatarRegistry : ScriptableObject
    {
        [SerializeField] private Avatar.Ref[] avatars;

        public bool TryGetAvatar(in Guid guid, out Avatar.Ref avatar)
        {
            var guidString = guid.ToString();
            avatar = avatars.FirstOrDefault(v => v.AssetGUID == guidString);
            return false;
        }

        public Avatar.Ref GetRandomAvatar()
        {
            if (avatars == null || avatars.Length == 0) return null;

            return avatars[Random.Range(0, avatars.Length)];
        }

        [Serializable]
        public sealed class Ref : ComponentReference<AvatarRegistry>
        {
            public Ref(string guid) : base(guid)
            {
            }
        }
    }
}