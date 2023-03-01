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

    public IReadOnlyList<string> GetAllDependencies(string assetBundle)
    {
        if (_dependencies == null)
            return null;

        List<string> allDependencies = new List<string>() { assetBundle }; // NOTE 为了防止存在循环依赖时，把自身也当做依赖加进去，在遍历完成后去掉
        RecursiveDependencies(assetBundle, allDependencies);
        allDependencies.Remove(assetBundle);
        return allDependencies;
    }

    private void RecursiveDependencies(string bundleName, List<string> allDependencies)
    {
        var directDependencies = GetDirectDependencies(bundleName);
        if (directDependencies != null)
        {
            foreach (var dependency in directDependencies)
            {
                if (allDependencies.Contains(dependency))
                    continue;
                allDependencies.Add(dependency);
                RecursiveDependencies(dependency, allDependencies);
            }
        }
    }
}
