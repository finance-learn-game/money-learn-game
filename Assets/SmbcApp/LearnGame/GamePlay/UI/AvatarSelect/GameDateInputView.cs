using System.Linq;
using Cysharp.Threading.Tasks;
using R3;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.Data;
using SmbcApp.LearnGame.GamePlay.Configuration;
using SmbcApp.LearnGame.UIWidgets.Date_Input;
using UnityEngine;
using VContainer;

namespace SmbcApp.LearnGame.GamePlay.UI.AvatarSelect
{
    internal sealed class GameDateInputView : MonoBehaviour
    {
        [SerializeField] [Required] private UIDateInput minDateInput;
        [SerializeField] [Required] private UIDateInput maxDateInput;

        private readonly ReactiveProperty<bool> _isValid = new();
        private UIDateInput.YearMonthDate _dbEndDate;
        private UIDateInput.YearMonthDate _dbStartDate;

        [Inject] internal MasterData MasterData;

        public ReadOnlyReactiveProperty<bool> IsValid => _isValid;
        public UIDateInput.YearMonthDate MinDate => minDateInput.Value.CurrentValue;
        public UIDateInput.YearMonthDate MaxDate => maxDateInput.Value.CurrentValue;

        private void Start()
        {
            StartAsync().Forget();
        }

        private async UniTask StartAsync()
        {
            minDateInput.Interactable = false;
            maxDateInput.Interactable = false;
            await UniTask.WaitUntil(() => MasterData.Initialized);
            minDateInput.Interactable = true;
            maxDateInput.Interactable = true;

            var chartOptions = GameConfiguration.Instance.StockChartOptions;
            Debug.Assert(chartOptions.Any(), "StockChartOptions is empty");

            var start = MasterData.DB.StockDataTable.SortByDate.First.Date
                .AddMonths(chartOptions.Max(op => op.Months));
            _dbStartDate = new UIDateInput.YearMonthDate(start.Year, start.Month);
            var end = MasterData.DB.StockDataTable.SortByDate.Last.Date;
            _dbEndDate = new UIDateInput.YearMonthDate(end.Year, end.Month);

            minDateInput.MinDate = maxDateInput.MinDate = _dbStartDate;
            minDateInput.MaxDate = maxDateInput.MaxDate = _dbEndDate;

            minDateInput.Value.Subscribe(OnDateChanged).AddTo(gameObject);
            maxDateInput.Value.Subscribe(OnDateChanged).AddTo(gameObject);
        }

        private void OnDateChanged(UIDateInput.YearMonthDate _)
        {
            _isValid.Value = minDateInput.Value.CurrentValue < maxDateInput.Value.CurrentValue &&
                             minDateInput.Value.CurrentValue >= _dbStartDate &&
                             maxDateInput.Value.CurrentValue <= _dbEndDate;
            minDateInput.MaxDate = maxDateInput.Value.CurrentValue;
            maxDateInput.MinDate = minDateInput.Value.CurrentValue;
        }
    }
}