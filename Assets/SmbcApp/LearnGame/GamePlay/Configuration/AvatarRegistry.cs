using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SmbcApp.LearnGame.GamePlay.Configuration
{
    [CreateAssetMenu(fileName = "Avatar Registry", menuName = "Configuration/Avatar Registry", order = 0)]
    public sealed class AvatarRegistry : ScriptableObject
    {
        [ValidateInput(nameof(ValidateAvatars))] [SerializeField]
        private Avatar[] avatars;

        public IReadOnlyList<Avatar> Avatars => avatars;

        private bool ValidateAvatars(Avatar[] self, ref string msg)
        {
            foreach (var avatar in self)
            {
                if (self.Count(v => v == avatar) <= 1) continue;

                msg = $"Duplicate avatar found: {avatar.name}";
                return false;
            }

            return true;
        }

        public bool TryGetAvatar(Guid guid, out Avatar avatar)
        {
            avatar = avatars.FirstOrDefault(v => v.Guid == guid);
            return avatar != null;
        }

        public Avatar GetRandomAvatar()
        {
            if (avatars == null || avatars.Length == 0) return null;

            return avatars[Random.Range(0, avatars.Length)];
        }
    }
}