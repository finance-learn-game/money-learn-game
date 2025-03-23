using System;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace SmbcApp.LearnGame.Utils.AddressableUtils
{
    public static class AddressableExt
    {
        public static HandleDisposable<T> Use<T>(this AsyncOperationHandle<T> handle)
        {
            return new HandleDisposable<T>(handle);
        }
    }

    public struct HandleDisposable<T> : IDisposable
    {
        private AsyncOperationHandle<T> _handle;
        public AsyncOperationHandle<T> Handle => _handle;

        public HandleDisposable(AsyncOperationHandle<T> handle)
        {
            _handle = handle;
        }

        public void Dispose()
        {
            if (!_handle.IsValid()) return;

            _handle.Release();
            _handle = default;
        }
    }
}