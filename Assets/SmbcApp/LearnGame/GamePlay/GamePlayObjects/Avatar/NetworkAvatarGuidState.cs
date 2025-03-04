using System;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.GamePlay.Configuration;
using SmbcApp.LearnGame.Infrastructure;
using Unity.Logging;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace SmbcApp.LearnGame.GamePlay.GamePlayObjects.Avatar
{
    internal sealed class NetworkAvatarGuidState : NetworkBehaviour
    {
        [SerializeField] [Required] private AvatarRegistry avatarRegistry;

        [NonSerialized] public readonly NetworkVariable<NetworkGuid> AvatarGuid = new();
        private Configuration.Avatar _avatar;

        public Configuration.Avatar GetRegisteredAvatar()
        {
            if (_avatar == null)
                RegisterAvatar(AvatarGuid.Value.ToGuid());

            return _avatar;
        }

        public void SetRandomAvatar()
        {
            var avatar = avatarRegistry.GetRandomAvatar();
            if (avatar == null)
            {
                Log.Error("Avatar not found!");
                return;
            }

            AvatarGuid.Value = avatar.Guid.ToNetworkGuid();
            Log.Info("Random avatar set: {0}", AvatarGuid.Value.ToString());
        }

        private void RegisterAvatar(Guid guid)
        {
            if (guid == Guid.Empty)
                return;

            if (!avatarRegistry.TryGetAvatar(guid, out _avatar)) Log.Error("Avatar not found!");
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(NetworkAvatarGuidState))]
    internal class NetworkAvatarGuidStateEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var networkAvatarGuidState = (NetworkAvatarGuidState)target;
            EditorGUILayout.LabelField("Avatar Guid", networkAvatarGuidState.AvatarGuid.Value.ToGuid().ToString());
        }
    }
#endif
}