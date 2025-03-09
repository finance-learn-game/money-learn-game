using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace SmbcApp.LearnGame.UIWidgets.Chart
{
    internal sealed class UIChartValueLabel : MonoBehaviour
    {
        [SerializeField] [Required] private TMP_Text valueLabel;

        public string Value
        {
            set => valueLabel.text = value;
        }

        public TextAlignmentOptions TextAlignment
        {
            set => valueLabel.alignment = value;
        }

        [Serializable]
        public class Ref : ComponentReference<UIChartValueLabel>
        {
            public Ref(string guid) : base(guid)
            {
            }
        }
    }
}