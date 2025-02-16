using System;
using UnityEngine;

namespace SmbcApp.LearnGame.Infrastructure
{
    public abstract class GuidScriptableObject : ScriptableObject
    {
        [SerializeField] private byte[] guid;

        public Guid Guid => new(guid);

        private void OnValidate()
        {
            if (guid == null || guid.Length == 0) guid = Guid.NewGuid().ToByteArray();
        }
    }
}