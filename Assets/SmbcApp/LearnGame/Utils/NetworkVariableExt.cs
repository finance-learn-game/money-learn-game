using System;
using R3;
using Unity.Netcode;

namespace SmbcApp.LearnGame.Utils
{
    /// <summary>
    ///     NetworkVariable、NetworkListの拡張メソッド
    /// </summary>
    public static class NetworkVariableExt
    {
        public static Observable<T> OnChangedAsObservable<T>(this NetworkVariable<T> self)
        {
            return Observable.Create<T, NetworkVariable<T>>(self, (observer, variable) =>
            {
                variable.OnValueChanged += Handle;
                return Disposable.Create(() => variable.OnValueChanged -= Handle);

                void Handle(T _, T next)
                {
                    observer.OnNext(next);
                }
            });
        }

        public static Observable<NetworkListEvent<T>> OnChangedAsObservable<T>(this NetworkList<T> self)
            where T : unmanaged, IEquatable<T>
        {
            return Observable.Create<NetworkListEvent<T>, NetworkList<T>>(self, (observer, list) =>
            {
                list.OnListChanged += observer.OnNext;
                return Disposable.Create(() => list.OnListChanged -= observer.OnNext);
            });
        }
    }
}