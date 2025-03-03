using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace SmbcApp.LearnGame.Utils
{
    public sealed class AsyncObjectPool<T>
    {
        private readonly int _capacity;
        private readonly Func<UniTask<T>> _createFunc;
        private readonly Action<T> _destroyAction;
        private readonly Action<T> _disableAction;
        private readonly Action<T> _enableAction;
        private readonly Stack<T> _pool;

        public AsyncObjectPool(
            Func<UniTask<T>> createFunc,
            Action<T> enableAction,
            Action<T> disableAction,
            Action<T> destroyAction,
            int initialCapacity = 10
        )
        {
            _createFunc = createFunc;
            _enableAction = enableAction;
            _disableAction = disableAction;
            _destroyAction = destroyAction;

            _capacity = initialCapacity;
            _pool = new Stack<T>(_capacity);
        }

        public async UniTask<T> Rent()
        {
            if (_pool.TryPop(out var item))
            {
                _enableAction(item);
                return item;
            }

            var obj = await _createFunc();
            _enableAction(obj);
            return obj;
        }

        public void Return(T item)
        {
            _disableAction(item);
            if (_pool.Count < _capacity)
                _pool.Push(item);
            else
                _destroyAction(item);
        }
    }
}