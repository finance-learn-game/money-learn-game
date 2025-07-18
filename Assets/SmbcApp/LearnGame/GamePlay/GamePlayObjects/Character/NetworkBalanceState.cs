﻿using R3;
using SmbcApp.LearnGame.GamePlay.Configuration;
using Unity.Netcode;
using UnityEngine;

namespace SmbcApp.LearnGame.GamePlay.GamePlayObjects.Character
{
    [DisallowMultipleComponent]
    internal sealed class NetworkBalanceState : NetworkBehaviour
    {
        private readonly NetworkVariable<int> _currentBalance = new();

        public int GaveSalary { get; private set; }

        public int CurrentBalance
        {
            get => _currentBalance.Value;
            set
            {
                if (IsServer) _currentBalance.Value = value;
            }
        }

        [Rpc(SendTo.Owner)]
        public void GiveSalaryRpc(int salary)
        {
            GaveSalary = salary;
        }

        public Observable<int> OnChangeAsObservable()
        {
            return Observable.Create<int, NetworkVariable<int>>(_currentBalance, (observer, variable) =>
            {
                observer.OnNext(variable.Value);
                variable.OnValueChanged += Handler;

                return Disposable.Create(() => variable.OnValueChanged -= Handler);

                void Handler(int _, int next)
                {
                    observer.OnNext(next);
                }
            });
        }

        public void InitializeBalance()
        {
            _currentBalance.Value = GameConfiguration.Instance.InitialBalance;
        }
    }
}