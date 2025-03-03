using System.Collections.Generic;
using R3;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace SmbcApp.LearnGame.UIWidgets.UI_Dropdown
{
    [RequireComponent(typeof(TMP_Dropdown))]
    public sealed class UIDropdown : MonoBehaviour
    {
        [SerializeField] [Required] private TMP_Dropdown dropdown;

        public int Value
        {
            get => dropdown.value;
            set => dropdown.value = value;
        }

        public List<TMP_Dropdown.OptionData> Options => dropdown.options;

        public Observable<int> OnValueChanged =>
            dropdown.onValueChanged.AsObservable(dropdown.destroyCancellationToken);
    }
}