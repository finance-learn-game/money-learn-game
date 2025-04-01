using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using R3;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.Data;
using SmbcApp.LearnGame.GamePlay.Configuration;
using SmbcApp.LearnGame.GamePlay.Domain;
using SmbcApp.LearnGame.GamePlay.GameState.NetworkData;
using SmbcApp.LearnGame.UIWidgets.Chart;
using SmbcApp.LearnGame.UIWidgets.Modal;
using SmbcApp.LearnGame.UIWidgets.UI_Dropdown;
using TMPro;
using UnityEngine;
using VContainer;

namespace SmbcApp.LearnGame.GamePlay.UI.Game
{
    internal sealed class StockChartModalView : UIModal
    {
        [SerializeField] [Required] private UIChart uiChart;
        [SerializeField] [Required] private UIDropdown rangeDropdown;
        [SerializeField] [Required] private TMP_Text titleText;

        private RangeOptionData[] _rangeOptions;

        [Inject] internal NetworkGameTurn GameTurn;
        [Inject] internal MasterData MasterData;
        [Inject] internal StockDomain StockDomain;

        protected override void Start()
        {
            base.Start();

            _rangeOptions = GameConfiguration.Instance.StockChartOptions
                .Select(op => new RangeOptionData(
                    op.Label,
                    () =>
                    {
                        var now = GameTurn.CurrentTime;
                        var min = now.AddMonths(-op.Months);
                        return (min, now);
                    }
                ))
                .ToArray();

            rangeDropdown.Options.Clear();
            rangeDropdown.Options.AddRange(_rangeOptions.Select(op => new TMP_Dropdown.OptionData(op.Label)));
            rangeDropdown.Value = 0;
            rangeDropdown.OnValueChanged.Subscribe(OnChangeDropdown).AddTo(gameObject);

            UniTask.Void(async () =>
            {
                await UniTask.WaitUntil(() => MasterData.Initialized);
                DrawChart();
            });
        }

        private void OnChangeDropdown(int index)
        {
            DrawChart();
        }

        private void DrawChart()
        {
            var option = _rangeOptions[rangeDropdown.Value];
            var range = option.DateRangeFunc();
            var organizations = MasterData.DB.OrganizationDataTable.All;
            uiChart.ChartGraphic.SetData(
                StockDataView()
                    .GroupBy(s => s.OrganizationId)
                    .OrderBy(g => g.Key)
                    .Select(group =>
                        new ChartGraphic.ChartData(
                            group.Select(g => (float)g.StockPrice).ToArray(), group.Key,
                            organizations[group.Key].Name
                        )
                    )
                    .ToArray()
            );
            uiChart.SetXLabels(XLabels()).Forget();
            titleText.text = $"株価の変動 ({range.Min:yyyy/MM} ~ {range.Max:yyyy/MM})";

            return;

            IEnumerable<StockData> StockDataView()
            {
                var stockDataTable = MasterData.DB.StockDataTable;
                for (var date = range.Min; date <= range.Max; date = date.AddMonths(1))
                {
                    var stockDate = new DateTime(date.Year, date.Month, 1);
                    foreach (var stockData in stockDataTable.FindClosestByDate(stockDate, false))
                        yield return stockData;
                }
            }

            IEnumerable<string> XLabels()
            {
                var year = -1;
                for (var date = range.Min; date <= range.Max; date = date.AddMonths(1))
                    if (date.Year != year)
                    {
                        year = date.Year;
                        yield return date.ToString("yyyy/MM");
                    }
                    else
                    {
                        yield return date.ToString("MM月");
                    }
            }
        }

        private readonly struct RangeOptionData
        {
            public readonly string Label;
            public readonly Func<(DateTime Min, DateTime Max)> DateRangeFunc;

            public RangeOptionData(string label, Func<(DateTime, DateTime)> dateRangeFunc)
            {
                Label = label;
                DateRangeFunc = dateRangeFunc;
            }
        }
    }
}