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

        public static int IndexOfClientId<T>(this NetworkList<T> self, ulong playerId)
            where T : unmanaged, IEquatable<T>, IDataWithClientId
        {
            for (var i = 0; i < self.Count; i++)
                if (self[i].ClientId == playerId)
                    return i;

            return -1;
        }
    }

    public interface IDataWithClientId
    {
        public ulong ClientId { get; }
    }
}