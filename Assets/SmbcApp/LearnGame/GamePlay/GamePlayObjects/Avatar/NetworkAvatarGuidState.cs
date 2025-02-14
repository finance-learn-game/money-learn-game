using System;
using Addler.Runtime.Core.LifetimeBinding;
using Cysharp.Threading.Tasks;
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
        [SerializeField] [Required] private AvatarRegistry.Ref avatarRegistryRef;

        private Configuration.Avatar _avatar;
        private AvatarRegistry _avatarRegistry;
        public bool IsInitialized { get; private set; }

        private async void Start()
        {
            try
            {
                _avatarRegistry = await avatarRegistryRef.LoadAssetAsync().BindTo(gameObject);
                IsInitialized = true;
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to load AvatarRegistry");
            }
        }

        public async UniTask<Configuration.Avatar> GetRegisteredAvatar()
        {
            if (!CheckInitialized()) return null;

            if (_avatar == null)
                await RegisterAvatar(avatarGuid.Value.ToGuid());

            return _avatar;
        }

        public void SetRandomAvatar()
        {
            if (!CheckInitialized()) return;

            avatarGuid.Value = _avatarRegistry.GetRandomAvatar().AssetGUID.ToNetworkGuid();
        }

        private async UniTask RegisterAvatar(Guid guid)
        {
            if (!CheckInitialized()) return;
            if (guid == Guid.Empty)
                return;

            if (!_avatarRegistry.TryGetAvatar(guid, out var avatar))
            {
                Log.Error("Avatar not found!");
                return;
            }

            if (_avatar != null)
                return;

            _avatar = await avatar.LoadAssetAsync().BindTo(gameObject);
        }

        private bool CheckInitialized()
        {
            if (!IsInitialized) Log.Error("AvatarRegistry is not initialized");
            return IsInitialized;
        }
    }
}