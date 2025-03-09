using System;
using R3;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SmbcApp.LearnGame.UIWidgets.Chart
{
    internal sealed class ChartLegendItem : MonoBehaviour
    {
        [SerializeField] [Required] private Image lineSample;
        [SerializeField] [Required] private Toggle showToggle;
        [SerializeField] [Required] private TMP_Text labelText;

        public Observable<bool> OnToggle => showToggle.OnValueChangedAsObservable();

        public bool IsShow
        {
            get => showToggle.isOn;
            set => showToggle.isOn = value;
        }

        public void Configure(string label, Color color)
        {
            labelText.text = string.IsNullOrEmpty(label) ? "不明" : label;
            lineSample.color = color;
        }

        [Serializable]
        public class Ref : ComponentReference<ChartLegendItem>
        {
            public Ref(string guid) : base(guid)
            {
            }
        }
    }
}