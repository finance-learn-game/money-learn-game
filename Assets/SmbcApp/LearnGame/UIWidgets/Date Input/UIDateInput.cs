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
    public sealed class UIDateInput : MonoBehaviour
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

        private readonly ReactiveProperty<YearMonthDate> _value = new();
        public ReadOnlyReactiveProperty<YearMonthDate> Value => _value;

        public bool Interactable
        {
            get => yearInput.interactable && monthInput.interactable;
            set
            {
                yearInput.interactable = value;
                monthInput.interactable = value;
            }
        }

        public YearMonthDate? MinDate
        {
            get => enableMinDate ? minDate : null;
            set
            {
                enableMinDate = value.HasValue;
                minDate = value ?? default;
                ValidateInput();
            }
        }

        public YearMonthDate? MaxDate
        {
            get => enableMaxDate ? maxDate : null;
            set
            {
                enableMaxDate = value.HasValue;
                maxDate = value ?? default;
                ValidateInput();
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
            var year = int.TryParse(yearInput.text, out var result) ? math.max(0, result) : 0;
            var month = int.TryParse(monthInput.text, out result) ? math.clamp(result, 1, 12) : 1;
            var date = new YearMonthDate(year, month);
            if (enableMinDate && date < minDate)
            {
                year = minDate.year;
                month = minDate.month;
            }

            if (enableMaxDate && date > maxDate)
            {
                year = maxDate.year;
                month = maxDate.month;
            }

            yearInput.text = year.ToString();
            monthInput.text = month.ToString();
            _value.Value = new YearMonthDate(year, month);
        }

        [Serializable]
        public struct YearMonthDate : IComparable<YearMonthDate>, IEquatable<YearMonthDate>
        {
            [ValidateInput(nameof(Validate))] public int year, month;

            private bool Validate()
            {
                return year > 0 && month is > 0 and <= 12;
            }

            public DateTime ToDateTime => new(year, month, 1);

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

            public static bool operator <(YearMonthDate left, YearMonthDate right)
            {
                return left.CompareTo(right) < 0;
            }

            public static bool operator >(YearMonthDate left, YearMonthDate right)
            {
                return left.CompareTo(right) > 0;
            }

            public static bool operator <=(YearMonthDate left, YearMonthDate right)
            {
                return left.CompareTo(right) <= 0;
            }

            public static bool operator >=(YearMonthDate left, YearMonthDate right)
            {
                return left.CompareTo(right) >= 0;
            }

            public bool Equals(YearMonthDate other)
            {
                return year == other.year && month == other.month;
            }

            public override bool Equals(object obj)
            {
                return obj is YearMonthDate other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(year, month);
            }
        }
    }
}