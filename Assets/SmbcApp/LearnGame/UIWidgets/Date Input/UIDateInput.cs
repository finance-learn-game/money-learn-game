using System;
using R3;
using Sirenix.OdinInspector;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

namespace SmbcApp.LearnGame.UIWidgets.Date_Input
{
    /// <summary>
    ///     日付入力用のウェジット
    /// </summary>
    internal sealed class UIDateInput : MonoBehaviour
    {
        [SerializeField] [Required] private TMP_Text labelText;
        [SerializeField] [Required] private TMP_InputField yearInput;
        [SerializeField] [Required] private TMP_InputField monthInput;
        [SerializeField] private bool enableMinDate;
        [SerializeField] private bool enableMaxDate;

        [ShowIf(nameof(enableMinDate))] [SerializeField]
        private YearMonthDate minDate;

        [ShowIf(nameof(enableMaxDate))] [SerializeField]
        private YearMonthDate maxDate;

        public YearMonthDate? MinDate
        {
            get => enableMinDate ? minDate : null;
            set
            {
                enableMinDate = value.HasValue;
                minDate = value ?? default;
            }
        }

        public YearMonthDate? MaxDate
        {
            get => enableMaxDate ? maxDate : null;
            set
            {
                enableMaxDate = value.HasValue;
                maxDate = value ?? default;
            }
        }

        public string Label
        {
            get => labelText.text;
            set => labelText.text = value;
        }

        private void Start()
        {
            yearInput.characterValidation = TMP_InputField.CharacterValidation.Integer;
            monthInput.characterValidation = TMP_InputField.CharacterValidation.Integer;
            Observable.Merge(
                    yearInput.onEndEdit.AsObservable(),
                    monthInput.onEndEdit.AsObservable()
                )
                .Prepend(string.Empty)
                .Subscribe(_ => ValidateInput())
                .AddTo(gameObject);
        }

        private void ValidateInput()
        {
            var year = math.max(0, int.Parse(yearInput.text));
            var month = math.clamp(int.Parse(monthInput.text), 1, 12);
            var date = new YearMonthDate(year, month);
            if (enableMinDate && date.CompareTo(minDate) < 0)
            {
                year = minDate.year;
                month = minDate.month;
            }

            if (enableMaxDate && date.CompareTo(maxDate) > 0)
            {
                year = maxDate.year;
                month = maxDate.month;
            }

            yearInput.text = year.ToString();
            monthInput.text = month.ToString();
        }

        [Serializable]
        public struct YearMonthDate : IComparable<YearMonthDate>
        {
            [ValidateInput(nameof(Validate))] public int year, month;

            private bool Validate()
            {
                return year > 0 && month is > 0 and <= 12;
            }

            public YearMonthDate(int year, int month)
            {
                this.year = year;
                this.month = month;
            }

            public int CompareTo(YearMonthDate other)
            {
                var yearComparison = year.CompareTo(other.year);
                return yearComparison != 0 ? yearComparison : month.CompareTo(other.month);
            }
        }
    }
}