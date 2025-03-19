using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SmbcApp.LearnGame.Utils
{
    public class SingletonScriptableObject<T> : ScriptableObject where T : SingletonScriptableObject<T>
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
#if UNITY_EDITOR
                if (_instance == null)
                    _instance = LoadAsset();
#else
                if (_instance == null)
                    _instance = CreateInstance<T>();
#endif
                return _instance;
            }
        }

        private void OnEnable()
        {
#if !UNITY_EDITOR
            _instance = (T)this;
#endif
        }

#if UNITY_EDITOR
        private static T LoadAsset()
        {
            var attr = typeof(T).GetCustomAttribute<SingletonScriptableAttribute>();
            if (attr != null)
                return Addressables.LoadAssetAsync<T>(attr.Address).WaitForCompletion();

            throw new Exception($"{typeof(T).Name} is not marked with SingletonScriptableAttribute");
        }
#endif
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SingletonScriptableAttribute : Attribute
    {
        public readonly string Address;

        public SingletonScriptableAttribute(string address)
        {
            Address = address;
        }
    }
}