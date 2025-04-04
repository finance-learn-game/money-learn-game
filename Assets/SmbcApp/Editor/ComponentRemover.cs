using UnityEditor;
using UnityEngine;

namespace SmbcApp.Editor
{
    internal static class ComponentRemover
    {
        [MenuItem("Assets/Remove Components/Collider")]
        private static void RemoveCollider()
        {
            RemoveComponents<Collider>();
        }

        private static void RemoveComponents<T>() where T : Component
        {
            var selectedObjects = Selection.gameObjects;
            foreach (var obj in selectedObjects)
            {
                var prefabType = PrefabUtility.GetPrefabAssetType(obj);
                if (prefabType is not (PrefabAssetType.Regular or PrefabAssetType.Variant)) continue;

                var prefabPath = AssetDatabase.GetAssetPath(obj);
                var prefab = PrefabUtility.LoadPrefabContents(prefabPath);

                var components = prefab.GetComponents<T>();
                foreach (var component in components) Object.DestroyImmediate(component);
                PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
                PrefabUtility.UnloadPrefabContents(prefab);
                Debug.Log($"Removed {typeof(T).Name} from prefab at {prefabPath}");
            }
        }
    }
}