using System;
using UnityEngine;

namespace SmbcApp.LearnGame.Utils
{
    [Serializable]
    public sealed class SpriteReference : ComponentReference<Sprite>
    {
        public SpriteReference(string guid) : base(guid)
        {
        }
    }
}