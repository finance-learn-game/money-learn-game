using System.Collections.Generic;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace SmbcApp.LearnGame.Utils
{
    public static class ListExt
    {
        public static void Shuffle<T>(this IList<T> self)
        {
            var rng = new Random((uint)(Time.time * 1000));
            for (var i = self.Count - 1; i > 0; i--)
            {
                var j = rng.NextInt(0, i + 1);
                (self[i], self[j]) = (self[j], self[i]);
            }
        }
    }
}