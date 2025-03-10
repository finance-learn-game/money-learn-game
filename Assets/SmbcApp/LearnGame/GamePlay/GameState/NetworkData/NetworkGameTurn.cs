using System;
using R3;
using Unity.Netcode;

namespace SmbcApp.LearnGame.GamePlay.GameState.NetworkData
{
    /// <summary>
    ///     インゲームでのターンを管理する
    /// </summary>
    internal sealed class NetworkGameTurn : NetworkBehaviour
    {
        private readonly NetworkVariable<long> _currentTimeTick = new();

        public DateTime CurrentTime
        {
            get => new(_currentTimeTick.Value);
            set
            {
                if (IsServer) _currentTimeTick.Value = value.Ticks;
            }
        }

        public Observable<DateTime> OnChangeAsObservable()
        {
            return Observable.Create<DateTime, NetworkVariable<long>>(_currentTimeTick, (observer, variable) =>
            {
                observer.OnNext(new DateTime(variable.Value));
                variable.OnValueChanged += Handler;

                return Disposable.Create(() => variable.OnValueChanged -= Handler);

                void Handler(long _, long next)
                {
                    observer.OnNext(new DateTime(next));
                }
            });
        }
    }
}