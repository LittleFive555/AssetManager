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

        AssetBundleBuild bundlesInfoBuild = CreateBundlesInfoBuild(assetBundleDirectory, manifest);
        BuildPipeline.BuildAssetBundles(assetBundleDirectory, new AssetBundleBuild[] { bundlesInfoBuild }, BuildAssetBundleOptions.None,
            BuildTarget.StandaloneWindows);
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
}