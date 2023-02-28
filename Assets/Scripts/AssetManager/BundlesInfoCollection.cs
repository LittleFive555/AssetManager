using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct BundleInfo
{
    public string Name;
    public string[] Assets;
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
}
