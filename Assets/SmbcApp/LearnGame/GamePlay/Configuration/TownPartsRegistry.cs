using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SmbcApp.LearnGame.GamePlay.Configuration
{
    [CreateAssetMenu(fileName = "Town Parts Registry", menuName = "Configuration/Town Parts Registry", order = 0)]
    public sealed class TownPartsRegistry : ScriptableObject
    {
        [ValidateInput(nameof(ValidateTownParts))] [SerializeField]
        private TownPart[] townParts;

        public IReadOnlyList<TownPart> TownParts => townParts;

        private bool ValidateTownParts(TownPart[] parts, ref string errorMessage)
        {
            // nameの重複チェック
            // priceとpointの正負チェック
            var names = new HashSet<string>();
            if (!parts.All(part => names.Add(part.Name)))
            {
                errorMessage = "Duplicate town part name found.";
                return false;
            }

            if (parts.All(part => part.Price >= 0)) return true;

            errorMessage = "Price must be greater than or equal to 0.";
            return false;
        }

        [Serializable]
        public class TownPart
        {
            [SerializeField] [Required] private string name;
            [SerializeField] [Required] private string description;
            [SerializeField] [Required] private AssetReferenceSprite thumbnail;
            [SerializeField] [Required] private AssetReferenceGameObject prefab;
            [SerializeField] private int price;
            [SerializeField] private int point;

            public string Name => name;
            public string Description => description;
            public AssetReferenceSprite Thumbnail => thumbnail;
            public AssetReferenceGameObject Prefab => prefab;
            public int Price => price;
            public int Point => point;
        }
    }
}