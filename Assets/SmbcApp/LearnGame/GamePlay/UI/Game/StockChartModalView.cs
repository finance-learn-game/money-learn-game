using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using R3;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.Data;
using SmbcApp.LearnGame.GamePlay.GameState.NetworkData;
using SmbcApp.LearnGame.UIWidgets.Chart;
using SmbcApp.LearnGame.UIWidgets.Modal;
using SmbcApp.LearnGame.UIWidgets.UI_Dropdown;
using TMPro;
using Unity.Logging;
using UnityEngine;
using VContainer;

namespace SmbcApp.LearnGame.GamePlay.UI.Game
{
    internal sealed class StockChartModalView : UIModal
    {
        [SerializeField] [Required] private ChartGraphic chartGraphic;
        [SerializeField] [Required] private UIDropdown rangeDropdown;

        private RangeOptionData[] _rangeOptions;

        [Inject] internal NetworkGameTime GameTime;
        [Inject] internal MasterData MasterData;

        protected override void Start()
        {
            base.Start();

            _rangeOptions = new[]
            {
                new RangeOptionData("過去6月", () =>
                {
                    var now = GameTime.CurrentTime;
                    var min = now.AddMonths(-6);
                    return (min, now);
                }),
                new RangeOptionData("過去12月", () =>
                {
                    var now = GameTime.CurrentTime;
                    var min = now.AddMonths(-12);
                    return (min, now);
                }),
                new RangeOptionData("過去18月", () =>
                {
                    var now = GameTime.CurrentTime;
                    var min = now.AddMonths(-18);
                    return (min, now);
                })
            };

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
            var stockTable = MasterData.DB.StockDataTable;

            var data = new List<float>();
            for (var date = range.Min; date <= range.Max; date = date.AddMonths(1))
            {
                var stock = stockTable.FindClosestByDateAndOrganizationId((date, 0), false);
                if (stock == null)
                {
                    Log.Warning($"Stock not found: {date}");
                    continue;
                }

                data.Add(stock.StockPrice);
            }

            chartGraphic.SetData(data.ToArray());
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