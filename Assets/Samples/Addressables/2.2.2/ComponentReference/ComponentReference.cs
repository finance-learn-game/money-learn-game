using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
///     Creates an AssetReference that is restricted to having a specific Component.
///     * This is the class that inherits from AssetReference.  It is generic and does not specify which Components it
///     might care about.  A concrete child of this class is required for serialization to work.
///     * At edit-time it validates that the asset set on it is a GameObject with the required Component.
///     * At runtime it can load/instantiate the GameObject, then return the desired component.  API matches base class
///     (LoadAssetAsync &amp; InstantiateAsync).
/// </summary>
/// <typeparam name="TComponent">The component type.</typeparam>
public class ComponentReference<TComponent> : AssetReference
{
    /// <inheritdoc />
    public ComponentReference(string guid) : base(guid)
    {
    }

    /// <inheritdoc />
    public new AsyncOperationHandle<TComponent> InstantiateAsync(Vector3 position, Quaternion rotation,
        Transform parent = null)
    {
        return Addressables.ResourceManager.CreateChainOperation(
            base.InstantiateAsync(position, Quaternion.identity, parent), GameObjectReady);
    }

    /// <inheritdoc />
    public new AsyncOperationHandle<TComponent> InstantiateAsync(Transform parent = null,
        bool instantiateInWorldSpace = false)
    {
        return Addressables.ResourceManager.CreateChainOperation(
            base.InstantiateAsync(parent, instantiateInWorldSpace), GameObjectReady);
    }

    /// <inheritdoc />
    public AsyncOperationHandle<TComponent> LoadAssetAsync()
    {
        return Addressables.ResourceManager.CreateChainOperation(
            base.LoadAssetAsync<GameObject>(), GameObjectReady);
    }

    /// <inheritdoc />
    private AsyncOperationHandle<TComponent> GameObjectReady(AsyncOperationHandle<GameObject> arg)
    {
        var comp = arg.Result.GetComponent<TComponent>();
        return Addressables.ResourceManager.CreateCompletedOperation(comp,
            string.Empty);
    }

    /// <summary>
    ///     Validates that the assigned asset has the component type
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool ValidateAsset(Object obj)
    {
        var go = obj as GameObject;
        return go != null && go.GetComponent<TComponent>() != null;
    }

    /// <summary>
    ///     Validates that the assigned asset has the component type, but only in the Editor
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public override bool ValidateAsset(string path)
    {
#if UNITY_EDITOR
        //this load can be expensive...
        var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        return go != null && go.GetComponent<TComponent>() != null;
#else
            return false;
#endif
    }

    /// <inheritdoc />
    public void ReleaseInstance(AsyncOperationHandle<TComponent> op)
    {
        // Release the instance
        var component = op.Result as Component;
        if (component != null) Addressables.ReleaseInstance(component.gameObject);

        // Release the handle
        op.Release();
    }
}