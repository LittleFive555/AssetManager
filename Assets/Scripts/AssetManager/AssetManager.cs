using System;

public interface IAssetLoader
{
    public T LoadAsset<T>(string path) where T : UnityEngine.Object;

    public T LoadAssetAsync<T>(string path) where T : UnityEngine.Object;

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
                _assetLoader = new BundleAssetLoader();
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

    public static T LoadAssetAsync<T>(string path) where T : UnityEngine.Object
    {
        return AssetLoader.LoadAssetAsync<T>(path);
    }
}
