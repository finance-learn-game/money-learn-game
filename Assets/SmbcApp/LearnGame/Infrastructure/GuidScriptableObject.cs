using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SmbcApp.LearnGame.Infrastructure
{
    public abstract class GuidScriptableObject : ScriptableObject
    {
        [SerializeField] [ReadOnly] private byte[] guid;

        [ShowInInspector]
        [ReadOnly]
        public Guid Guid => guid != null && guid.Length != 0
            ? new Guid(guid)
            : Guid.Empty;

        private void OnValidate()
        {
            if (guid == null || guid.Length == 0) guid = Guid.NewGuid().ToByteArray();
        }
    }
}