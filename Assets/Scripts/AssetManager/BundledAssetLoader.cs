using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BundledAssetLoader : IAssetLoader
{
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
    private bool _initialized = false;

    private Dictionary<object, LoadedAsset> _loadedAssets = new Dictionary<object, LoadedAsset>();


    private void Initialize()
    {
        _initialized = true;

        var infoBundle = AssetBundle.LoadFromFile(AssetConstPath.BundleInfoPath);
        var bundlesInfoCollection = infoBundle.LoadAsset<BundlesInfoCollection>(AssetConstPath.BundleInfoAssetName);
        infoBundle.Unload(false);
        _assetBundleMapping = bundlesInfoCollection.CreateAssetBundleMapping();
    }

    public T LoadAsset<T>(string path) where T : UnityEngine.Object
    {
        var bundleName = AssetBundleMapping.GetBundleName(path);
        if (string.IsNullOrEmpty(bundleName))
            return null;

        AssetBundle assetBundle = AssetBundleManager.LoadBundleAndDependencies(bundleName);
        var asset = assetBundle.LoadAsset<T>(path);
        if (!_loadedAssets.TryGetValue(asset, out var loadedAsset))
            _loadedAssets.Add(asset, new LoadedAsset(asset, path));
        else
            loadedAsset.UseCount++;
        return asset;
    }

    public IEnumerator LoadAssetAsync<T>(string path, Action<T> onComplete) where T : UnityEngine.Object
    {
        var bundleName = AssetBundleMapping.GetBundleName(path);
        if (string.IsNullOrEmpty(bundleName))
            yield break;

        AssetBundle assetBundle = null;
        yield return AssetBundleManager.LoadBundleAndDependenciesAsync(bundleName, (bundle) => assetBundle = bundle);

        var request = assetBundle.LoadAssetAsync<T>(path);
        yield return request;
        var asset = request.asset as T;
        if (!_loadedAssets.TryGetValue(asset, out var loadedAsset))
            _loadedAssets.Add(asset, new LoadedAsset(asset, path));
        else
            loadedAsset.UseCount++;
        onComplete?.Invoke(asset);
    }

    public void UnloadAsset<T>(T asset) where T : UnityEngine.Object
    {
        if (!_loadedAssets.TryGetValue(asset, out var loadedAsset))
        {
            Debug.LogWarningFormat("Trying to unload an asset not loaded before. Asset Name:{0}", asset.name);
            return;
        }
        loadedAsset.UseCount--;
        if (loadedAsset.UseCount <= 0)
            _loadedAssets.Remove(asset);

        var bundleName = AssetBundleMapping.GetBundleName(loadedAsset.AssetPath);
        Debug.Assert(!string.IsNullOrEmpty(bundleName));
        AssetBundleManager.UnloadBundleAndDependencies(bundleName);
    }

    private class LoadedAsset
    {
        private object _asset;
        public object Asset => _asset;

        private string _assetPath;
        public string AssetPath => _assetPath;

        public int UseCount;

        public LoadedAsset(object asset, string assetPath)
        {
            _asset = asset;
            _assetPath = assetPath;
            UseCount = 1;
        }
    }
}
