using System.Collections.Generic;

public class AssetBundleDependencies
{
    private IReadOnlyDictionary<string, IReadOnlyList<string>> _dependencies;

    public AssetBundleDependencies(Dictionary<string, IReadOnlyList<string>> dependencies)
    {
        _dependencies = dependencies;
    }

    public IReadOnlyList<string> GetDirectDependencies(string assetBundle)
    {
        if (_dependencies == null)
            return null;
        return _dependencies.TryGetValue(assetBundle, out var directDependencies) ? directDependencies : null;
    }
}
