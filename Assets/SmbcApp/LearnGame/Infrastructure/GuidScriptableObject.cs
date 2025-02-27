using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SmbcApp.LearnGame.Infrastructure
{
    public abstract class GuidScriptableObject : ScriptableObject
    {
        [SerializeField] [ReadOnly] private byte[] guid;

        [ShowInInspector] [ReadOnly] public Guid Guid => new(guid);

        private void OnValidate()
        {
            if (guid == null || guid.Length == 0) guid = Guid.NewGuid().ToByteArray();
        }
    }
}