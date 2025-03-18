using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SmbcApp.LearnGame.GamePlay.Configuration
{
    /// <summary>
    ///     給料計算に関する設定を提供する
    /// </summary>
    [Serializable]
    public class SalaryConfig
    {
        [SerializeReference] private ISalaryOp[] salaryOps = Array.Empty<ISalaryOp>();

        public int CalculateSalary(DateTime gameStart, DateTime current)
        {
            return salaryOps.Sum(op => op.CalculateSalary(gameStart, current));
        }

        #region Ops

        private interface ISalaryOp
        {
            public int CalculateSalary(in DateTime gameStart, in DateTime current);
        }

        [Serializable]
        private class FixedSalaryOp : ISalaryOp
        {
            [SerializeField] private int salary;

            public int CalculateSalary(in DateTime gameStart, in DateTime current)
            {
                return salary;
            }
        }

        [Serializable]
        private class RandomSalaryOp : ISalaryOp
        {
            [SerializeField] private int min;
            [SerializeField] private int max;
            [SerializeField] private int minPeriod = 1;
            [SerializeField] private int maxPeriod = 1;

            public int CalculateSalary(in DateTime gameStart, in DateTime current)
            {
                var period = Random.Range(minPeriod, maxPeriod);

                var monthDiff = (current.Year - gameStart.Year) * 12 + current.Month - gameStart.Month;
                return monthDiff % period != 0 ? 0 : Random.Range(min, max);
            }
        }

        #endregion
    }
}