using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AssetBundleLoader : IAssetBundleLoader
{
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

    private Dictionary<string, LoadedBundle> _loadedBundles = new Dictionary<string, LoadedBundle>();
    private Dictionary<string, List<Action<AssetBundle>>> _bundleLoadCallbacks = new Dictionary<string, List<Action<AssetBundle>>>();

    private void Initialize()
    {
        _initialized = true;

        var infoBundle = AssetBundle.LoadFromFile(AssetConstPath.BundleInfoPath);
        var bundlesInfoCollection = infoBundle.LoadAsset<BundlesInfoCollection>(AssetConstPath.BundleInfoAssetName);
        infoBundle.Unload(false);
        _assetBundleDependencies = bundlesInfoCollection.CreateAssetBundleDependencies();
    }

    public AssetBundle LoadBundleAndDependencies(string assetBundleName)
    {
        AssetBundle assetBundle = LoadOrGetAssetBundle(assetBundleName);
        var allDependencies = AssetBundleDependencies.GetAllDependencies(assetBundleName);
        if (allDependencies != null && allDependencies.Count > 0)
        {
            foreach (var dependency in allDependencies)
                LoadOrGetAssetBundle(dependency);
        }
        LogCurrentAssetBundleStatus();
        return assetBundle;
    }

    public IEnumerator LoadBundleAndDependenciesAsync(string assetBundleName, Action<AssetBundle> onComplete)
    {
        yield return LoadAssetBundleAsync(assetBundleName, onComplete);
        var allDependencies = AssetBundleDependencies.GetAllDependencies(assetBundleName);
        if (allDependencies != null && allDependencies.Count > 0)
        {
            foreach (var dependency in allDependencies)
                yield return LoadAssetBundleAsync(dependency, null); // 这里依次加载所有的依赖，考虑是否可以同时加载所有的依赖？是否能更快？
        }
        LogCurrentAssetBundleStatus();
    }

    public void UnloadBundleAndDependencies(string assetBundleName)
    {
        var allDependencies = AssetBundleDependencies.GetAllDependencies(assetBundleName);
        DecrementCountOrUnloadAssetBundle(assetBundleName);
        foreach (var bundleName in allDependencies)
            DecrementCountOrUnloadAssetBundle(bundleName);

        LogCurrentAssetBundleStatus();
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
            var bundlePath = Path.Combine(AssetConstPath.BundlePath, bundleName);
            assetBundle = AssetBundle.LoadFromFile(bundlePath);
            _loadedBundles.Add(bundleName, new LoadedBundle(assetBundle));
        }

        return assetBundle;
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

        var bundlePath = Path.Combine(AssetConstPath.BundlePath, bundleName);
        var request = AssetBundle.LoadFromFileAsync(bundlePath);
        yield return request;

        _loadedBundles.Add(bundleName, new LoadedBundle(request.assetBundle));
        foreach (var callback in _bundleLoadCallbacks[bundleName])
            callback?.Invoke(request.assetBundle);
        _bundleLoadCallbacks.Remove(bundleName);
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
