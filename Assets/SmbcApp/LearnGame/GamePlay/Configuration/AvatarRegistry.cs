using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SmbcApp.LearnGame.GamePlay.Configuration
{
    [CreateAssetMenu(fileName = "Avatar Registry", menuName = "Configuration/Avatar Registry", order = 0)]
    internal sealed class AvatarRegistry : ScriptableObject
    {
        [SerializeField] private Avatar[] avatars;

        public bool TryGetAvatar(Guid guid, out Avatar avatar)
        {
            avatar = avatars.FirstOrDefault(v => v.Guid == guid);
            return false;
        }

        public Avatar GetRandomAvatar()
        {
            if (avatars == null || avatars.Length == 0) return null;

            return avatars[Random.Range(0, avatars.Length)];
        }
    }
}