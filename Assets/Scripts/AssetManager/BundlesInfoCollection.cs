using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct BundleInfo
{
    public string Name;
    public string[] Assets;
    public string[] Dependencies;
}

public class BundlesInfoCollection : ScriptableObject
{
    [SerializeField]
    private List<BundleInfo> _bundlesInfo;

    public void AddBundleInfo(BundleInfo bundleInfo)
    {
        if (_bundlesInfo == null)
            _bundlesInfo = new List<BundleInfo>();
        _bundlesInfo.Add(bundleInfo);
    }
    
    public AssetBundleMapping CreateAssetBundleMapping()
    {
        Dictionary<string, string> assetToBundle = new Dictionary<string, string>();
        foreach (var bundleInfo in _bundlesInfo)
        {
            foreach (var asset in bundleInfo.Assets)
                assetToBundle.Add(asset, bundleInfo.Name);
        }
        return new AssetBundleMapping(assetToBundle);
    }

    public AssetBundleDependencies CreateAssetBundleDependencies()
    {
        Dictionary<string, IReadOnlyList<string>> assetToBundle = new Dictionary<string, IReadOnlyList<string>>();
        foreach (var bundleInfo in _bundlesInfo)
            assetToBundle.Add(bundleInfo.Name, new List<string>(bundleInfo.Dependencies));
        return new AssetBundleDependencies(assetToBundle);
    }
}
