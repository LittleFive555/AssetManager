using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class CreateAssetBundles
{
    [MenuItem("Assets/Build AssetBundles")]
    static void BuildAllAssetBundles()
    {
        string assetBundleDirectory = "Assets/AssetBundles";
        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }
        var manifest = BuildPipeline.BuildAssetBundles(assetBundleDirectory,
                                        BuildAssetBundleOptions.None,
                                        BuildTarget.StandaloneWindows);

        CheckCircularDependenciesAndWarning(manifest);

        AssetBundleBuild bundlesInfoBuild = CreateBundlesInfoBuild(assetBundleDirectory, manifest);
        BuildPipeline.BuildAssetBundles(assetBundleDirectory, new AssetBundleBuild[] { bundlesInfoBuild }, BuildAssetBundleOptions.None,
            BuildTarget.StandaloneWindows);
    }

    private static void CheckCircularDependenciesAndWarning(AssetBundleManifest manifest)
    {
        var circularDependencies = CheckCircularDependencies(manifest);
        if (circularDependencies != null && circularDependencies.Count > 0)
        {
            for (int i = 0; i < circularDependencies.Count; i++)
            {
                var circularDependency = circularDependencies[i];
                string logInfo = "Circular Dependency " + i + ": ";
                foreach (var dependency in circularDependency)
                    logInfo += dependency + "->";
                Debug.LogWarning(logInfo);
            }
        }
    }

    private static AssetBundleBuild CreateBundlesInfoBuild(string assetBundleDirectory, AssetBundleManifest manifest)
    {
        BundlesInfoCollection bundlesInfoCollection = ScriptableObject.CreateInstance<BundlesInfoCollection>();
        foreach (var assetBundleName in manifest.GetAllAssetBundles())
        {
            var assetBundle = AssetBundle.LoadFromFile(Path.Combine(assetBundleDirectory, assetBundleName));
            bundlesInfoCollection.AddBundleInfo(new BundleInfo() 
            {
                Name = assetBundleName,
                Assets = assetBundle.GetAllAssetNames(), // NOTE assetBundle.GetAllAssetNames()获得的名字全小写的
                Dependencies = manifest.GetDirectDependencies(assetBundleName)
            });
            assetBundle.Unload(true);
        }
        var assetName = "Assets/BundlesInfo.asset";
        AssetDatabase.CreateAsset(bundlesInfoCollection, assetName);
        AssetDatabase.SaveAssets();
        AssetBundleBuild bundlesInfoBuild = new AssetBundleBuild
        {
            assetBundleName = "bundleinfo",
            assetNames = new string[] { assetName },
        };
        return bundlesInfoBuild;
    }

    private static List<List<string>> CheckCircularDependencies(AssetBundleManifest manifest)
    {
        Dictionary<string, HashSet<string>> dependencyGraph = new Dictionary<string, HashSet<string>>();

        string[] bundleNames = manifest.GetAllAssetBundles();
        foreach (string bundleName in bundleNames)
        {
            string[] dependencies = manifest.GetDirectDependencies(bundleName);
            dependencyGraph.Add(bundleName, new HashSet<string>(dependencies));
        }

        List<List<string>> circularDependencies = new List<List<string>>();

        foreach (string bundleName in bundleNames)
        {
            Stack<string> path = new Stack<string>();
            HashSet<string> visited = new HashSet<string>();
            CheckCircularDependenciesRecursive(bundleName, path, visited, dependencyGraph, circularDependencies);
        }

        return circularDependencies;
    }

    private static void CheckCircularDependenciesRecursive(string bundleName, Stack<string> path, HashSet<string> visited, Dictionary<string, HashSet<string>> dependencyGraph, List<List<string>> circularDependencies)
    {
        if (visited.Contains(bundleName))
        {
            if (path.Contains(bundleName))
            {
                List<string> circularPath = new List<string>();
                circularPath.AddRange(path);
                circularPath.Reverse();
                circularPath.Add(bundleName);
                circularDependencies.Add(circularPath);
            }
            return;
        }

        visited.Add(bundleName);
        path.Push(bundleName);

        HashSet<string> dependencies;
        if (!dependencyGraph.TryGetValue(bundleName, out dependencies))
        {
            dependencies = new HashSet<string>();
        }

        foreach (string dependency in dependencies)
        {
            CheckCircularDependenciesRecursive(dependency, path, visited, dependencyGraph, circularDependencies);
        }

        path.Pop();
        visited.Remove(bundleName);
    }
}