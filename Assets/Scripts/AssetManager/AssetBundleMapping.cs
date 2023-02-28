using System.Collections.Generic;

public class AssetBundleMapping
{
    private IReadOnlyDictionary<string, string> _assetToBundle;

    public AssetBundleMapping(Dictionary<string, string> mapping)
    {
        _assetToBundle = mapping;
    }
    
    public string GetBundleName(string assetPath)
    {
        if (_assetToBundle == null)
            return null;
        return _assetToBundle.TryGetValue(assetPath.ToLower(), out var bundleName) ? bundleName : null;
    }
}
