using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace SmbcApp.LearnGame.UIWidgets.Slider
{
    public sealed class UINoInteractSlider : MonoBehaviour
    {
        [SerializeField] [Required] private UnityEngine.UI.Slider slider;
        [SerializeField] private TMP_Text labelText;

        public float Value
        {
            get => slider.value;
            set => slider.value = value;
        }

        public string Label
        {
            get => labelText.text;
            set => labelText.text = value;
        }
    }
}