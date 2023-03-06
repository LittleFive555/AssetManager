using System;
using System.Collections;
using System.Collections.Generic;
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

    private Dictionary<string, LoadedBundle> _loadedBundles = new Dictionary<string, LoadedBundle>();
    private Dictionary<object, LoadedAsset> _loadedAssets = new Dictionary<object, LoadedAsset>();

    private Dictionary<string, List<Action<AssetBundle>>> _bundleLoadCallbacks = new Dictionary<string, List<Action<AssetBundle>>>();
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
        float startTime = Time.realtimeSinceStartup;

        AssetBundle assetBundle = LoadRelativeBundles(path);
        LogCurrentAssetBundleStatus();

        var asset = assetBundle.LoadAsset<T>(path);
        if (!_loadedAssets.TryGetValue(asset, out var loadedAsset))
            _loadedAssets.Add(asset, new LoadedAsset(asset, path));
        else
            loadedAsset.UseCount++;

        Debug.LogFormat("Sync cost time: {0}", Time.realtimeSinceStartup - startTime);
        return asset;
    }

    public IEnumerator LoadAssetAsync<T>(string path, Action<T> onComplete) where T : UnityEngine.Object
    {
        float startTime = Time.realtimeSinceStartup;

        AssetBundle assetBundle = null;
        yield return LoadRelativeBundlesAsync(path, (bundle) => assetBundle = bundle);
        Debug.LogFormat("Async load bundle cost time: {0}", Time.realtimeSinceStartup - startTime);
        startTime = Time.realtimeSinceStartup;

        var request = assetBundle.LoadAssetAsync<T>(path);
        yield return request;
        var asset = request.asset as T;
        if (!_loadedAssets.TryGetValue(asset, out var loadedAsset))
            _loadedAssets.Add(asset, new LoadedAsset(asset, path));
        else
            loadedAsset.UseCount++;
        onComplete?.Invoke(asset);

        Debug.LogFormat("Async load asset cost time: {0}", Time.realtimeSinceStartup - startTime);
        LogCurrentAssetBundleStatus();
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
        UnloadRelativeBundle(loadedAsset.AssetPath);
        LogCurrentAssetBundleStatus();
    }

    private AssetBundle LoadRelativeBundles(string assetPath)
    {
        var bundleName = AssetBundleMapping.GetBundleName(assetPath);
        AssetBundle assetBundle = LoadOrGetAssetBundle(bundleName);
        var allDependencies = AssetBundleDependencies.GetAllDependencies(bundleName);
        if (allDependencies != null && allDependencies.Count > 0)
        {
            foreach (var dependency in allDependencies)
                LoadOrGetAssetBundle(dependency);
        }

        return assetBundle;
    }

    private AssetBundle LoadOrGetAssetBundle(string bundleName)
    {
        AssetBundle assetBundle;
        if (_loadedBundles.TryGetValue(bundleName, out var loadedBundle))
        {
            loadedBundle.UseCount++;
            assetBundle = loadedBundle.AssetBundle;
        }
        else
        {
            var bundlePath = Path.Combine(BundlePath, bundleName);
            assetBundle = AssetBundle.LoadFromFile(bundlePath);
            _loadedBundles.Add(bundleName, new LoadedBundle(assetBundle));
        }

        return assetBundle;
    }

    private IEnumerator LoadRelativeBundlesAsync(string assetPath, Action<AssetBundle> onComplete)
    {
        var bundleName = AssetBundleMapping.GetBundleName(assetPath);
        yield return LoadAssetBundleAsync(bundleName, onComplete);
        var allDependencies = AssetBundleDependencies.GetAllDependencies(bundleName);
        if (allDependencies != null && allDependencies.Count > 0)
        {
            foreach (var dependency in allDependencies)
                yield return LoadAssetBundleAsync(dependency, null); // 依次加载，考虑是否可以添加一个同时加载
        }
    }

    private IEnumerator LoadAssetBundleAsync(string bundleName, Action<AssetBundle> onComplete)
    {
        if (_loadedBundles.TryGetValue(bundleName, out var loadedBundle))
        {
            loadedBundle.UseCount++;
            onComplete?.Invoke(loadedBundle.AssetBundle);
            yield break;
        }

        if (_bundleLoadCallbacks.TryGetValue(bundleName, out var callbackList))
        {
            onComplete += (bundle) =>
            {
                if (_loadedBundles.TryGetValue(bundleName, out var loadedBundle))
                    loadedBundle.UseCount++;
                else
                    _loadedBundles.Add(bundleName, new LoadedBundle(bundle));
            };
            callbackList.Add(onComplete);
            yield break;
        }

        callbackList = new List<Action<AssetBundle>>();
        callbackList.Add(onComplete);
        _bundleLoadCallbacks[bundleName] = callbackList;

        var bundlePath = Path.Combine(BundlePath, bundleName);
        var request = AssetBundle.LoadFromFileAsync(bundlePath);
        yield return request;

        _loadedBundles.Add(bundleName, new LoadedBundle(request.assetBundle));
        foreach (var callback in _bundleLoadCallbacks[bundleName])
            callback?.Invoke(request.assetBundle);
        _bundleLoadCallbacks.Remove(bundleName);
    }

    private void UnloadRelativeBundle(string assetPath)
    {
        var directBundleName = AssetBundleMapping.GetBundleName(assetPath);
        var allDependencies = AssetBundleDependencies.GetAllDependencies(directBundleName);
        DecrementCountOrUnloadAssetBundle(directBundleName);
        foreach (var bundleName in allDependencies)
            DecrementCountOrUnloadAssetBundle(bundleName);
    }

    private void DecrementCountOrUnloadAssetBundle(string bundleName)
    {
        if (!_loadedBundles.TryGetValue(bundleName, out var loadedBundle))
        {
            Debug.LogWarningFormat("Trying to unload a AssetBundle not loaded before. AssetBundle Name:{0}", bundleName);
            return;
        }

        loadedBundle.UseCount--;
        if (loadedBundle.UseCount <= 0)
        {
            loadedBundle.AssetBundle.Unload(true);
            _loadedBundles.Remove(bundleName);
        }
    }

    private void LogCurrentAssetBundleStatus()
    {
        string message = "Bundles: ";
        foreach (var loadedBundle in _loadedBundles)
            message += "\n" + loadedBundle.Key + ", count: " + loadedBundle.Value.UseCount;
        Debug.Log(message);
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

    private class LoadedBundle
    {
        private AssetBundle _assetBundle;
        public AssetBundle AssetBundle => _assetBundle;

        public int UseCount;

        /// <summary>
        /// Use count will set 1 in constructor.
        /// </summary>
        /// <param name="assetBundle"></param>
        public LoadedBundle(AssetBundle assetBundle)
        {
            _assetBundle = assetBundle;
            UseCount = 1;
        }
    }
}
