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
            if (!_initialized)
                Initialize();
            return _assetBundleMapping;
        }
    }

    private AssetBundleDependencies _assetBundleDependencies;
    public AssetBundleDependencies AssetBundleDependencies
    {
        get
        {
            if (!_initialized)
                Initialize();
            return _assetBundleDependencies;
        }
    }

    private bool _initialized = false;

    private void Initialize()
    {
        _initialized = true;

        var infoBundle = AssetBundle.LoadFromFile(Path.Combine(BundlePath, BundleInfoName));
        var bundlesInfoCollection = infoBundle.LoadAsset<BundlesInfoCollection>("BundlesInfo");
        _assetBundleMapping = bundlesInfoCollection.CreateAssetBundleMapping();
        _assetBundleDependencies = bundlesInfoCollection.CreateAssetBundleDependencies();
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
