using System;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.GamePlay.Configuration;
using SmbcApp.LearnGame.Infrastructure;
using Unity.Logging;
using Unity.Netcode;
using UnityEngine;

namespace SmbcApp.LearnGame.GamePlay.GamePlayObjects.Avatar
{
    internal sealed class NetworkAvatarGuidState : NetworkBehaviour
    {
        [ReadOnly] public NetworkVariable<NetworkGuid> avatarGuid = new();
        [SerializeField] [Required] private AvatarRegistry avatarRegistry;

        private Configuration.Avatar _avatar;

        public Configuration.Avatar GetRegisteredAvatar()
        {
            if (_avatar == null)
                RegisterAvatar(avatarGuid.Value.ToGuid());

            return _avatar;
        }

        public void SetRandomAvatar()
        {
            avatarGuid.Value = avatarRegistry.GetRandomAvatar().Guid.ToNetworkGuid();
        }

        private void RegisterAvatar(Guid guid)
        {
            if (guid == Guid.Empty)
                return;

            if (!avatarRegistry.TryGetAvatar(guid, out _avatar)) Log.Error("Avatar not found!");
        }
    }
}