using System.IO;
using UnityEngine;

public class BundleAssetLoader : IAssetLoader
{
    private const string BundlePath = "Assets/AssetBundles";
    private const string BundleInfoName = "bundleinfo";

    private AssetBundleMapping _assetBundleMapping;
    private AssetBundleMapping AssetBundleMapping
    {
        get
        {
            if (_assetBundleMapping == null)
            {
                var infoBundle = AssetBundle.LoadFromFile(Path.Combine(BundlePath, BundleInfoName));
                _assetBundleMapping = infoBundle.LoadAsset<BundlesInfoCollection>("BundlesInfo").CreateAssetBundleMapping();
            }
            return _assetBundleMapping;
        }
    }

    public T LoadAsset<T>(string path) where T : UnityEngine.Object
    {
        var myLoadedAssetBundle = AssetBundle.LoadFromFile(Path.Combine(BundlePath, AssetBundleMapping.GetBundleName(path)));
        if (myLoadedAssetBundle == null)
        {
            Debug.Log("Failed to load AssetBundle!");
            return null;
        }
        var name = Path.GetFileName(path);
        return myLoadedAssetBundle.LoadAsset<T>(name);
    }

    public T LoadAssetAsync<T>(string path) where T : UnityEngine.Object
    {
        return LoadAsset<T>(path);
    }
}
