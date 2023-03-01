using System.IO;
using UnityEngine;

public class BundleAssetLoader : IAssetLoader
{
    private static readonly string BundlePath = Path.Combine("Assets", AssetBundlesDirName);
    private const string AssetBundlesDirName = "AssetBundles";
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
                infoBundle.Unload(false);
            }
            return _assetBundleMapping;
        }
    }

    private AssetBundleDependencies _assetBundleDependencies;
    public AssetBundleDependencies AssetBundleDependencies
    {
        get
        {
            if (_assetBundleDependencies == null)
            {
                var infoBundle = AssetBundle.LoadFromFile(Path.Combine(BundlePath, BundleInfoName));
                _assetBundleDependencies = infoBundle.LoadAsset<BundlesInfoCollection>("BundlesInfo").CreateAssetBundleDependencies();
                infoBundle.Unload(false);
            }
            return _assetBundleDependencies;
        }
    }

    public T LoadAsset<T>(string path) where T : UnityEngine.Object
    {
        var bundleName = AssetBundleMapping.GetBundleName(path);
        var bundlePath = Path.Combine(BundlePath, bundleName);
        var loadedAssetBundle = AssetBundle.LoadFromFile(bundlePath);
        if (loadedAssetBundle == null)
        {
            Debug.Log("Failed to load AssetBundle!");
            return null;
        }
        var allDependencies = AssetBundleDependencies.GetAllDependencies(bundleName);
        if (allDependencies != null && allDependencies.Count > 0)
        {
            foreach (var dependency in allDependencies)
                AssetBundle.LoadFromFile(Path.Combine(BundlePath, dependency));
        }
        return loadedAssetBundle.LoadAsset<T>(path);
    }

    public T LoadAssetAsync<T>(string path) where T : UnityEngine.Object
    {
        return LoadAsset<T>(path);
    }
}
