using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SmbcApp.LearnGame.Utils
{
    public class SingletonScriptableObject<T> : ScriptableObject where T : SingletonScriptableObject<T>
    {
        private static T _instance;

        public static T Instance => GetInstance();

        private static T GetInstance()
        {
            if (_instance != null) return _instance;

            var attr = typeof(T).GetCustomAttribute<SingletonScriptableAttribute>();
            if (attr == null)
            {
                Debug.LogError($"{typeof(T).Name} is not marked with SingletonScriptableAttribute");
                return null;
            }

            _instance = Addressables.LoadAssetAsync<T>(attr.Address).WaitForCompletion();
            return _instance;
        }
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