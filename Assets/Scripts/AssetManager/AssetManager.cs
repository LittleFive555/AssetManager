using System;
using System.Collections;

public interface IAssetLoader
{
    public T LoadAsset<T>(string path) where T : UnityEngine.Object;

    public IEnumerator LoadAssetAsync<T>(string path, Action<T> onComplete) where T : UnityEngine.Object;

    public void UnloadAsset<T>(T asset) where T : UnityEngine.Object;
}

public class AssetManager
{
    private static IAssetLoader _assetLoader;
    private static IAssetLoader AssetLoader
    {
        get
        {
#if UNITY_EDITOR
            if (_assetLoader == null)
                _assetLoader = new BundledAssetLoader();
#endif
            if (_assetLoader == null)
                throw new Exception();
            return _assetLoader;
        }
    }
    
    public static T LoadAsset<T>(string path) where T : UnityEngine.Object
    {
        return AssetLoader.LoadAsset<T>(path);
    }

    public static IEnumerator LoadAssetAsync<T>(string path, Action<T> onComplete) where T : UnityEngine.Object
    {
        return AssetLoader.LoadAssetAsync<T>(path, onComplete);
    }

    public static void UnloadAsset<T>(T obj) where T : UnityEngine.Object
    {
        AssetLoader.UnloadAsset(obj);
    }
}
