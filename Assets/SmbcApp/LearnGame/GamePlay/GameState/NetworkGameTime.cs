using System;
using R3;
using Unity.Netcode;

namespace SmbcApp.LearnGame.Gameplay.GameState
{
    internal sealed class NetworkGameTime : NetworkBehaviour
    {
        private readonly NetworkVariable<long> _currentTimeTick = new();

        public DateTime CurrentTime
        {
            get => new(_currentTimeTick.Value);
            set => _currentTimeTick.Value = value.Ticks;
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