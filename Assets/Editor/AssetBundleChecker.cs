using System.IO;
using UnityEditor;
using UnityEngine;

public class AssetBundleChecker
{
    [MenuItem("Assets/Get Dependencies")]
    private static void GetDependenciesSelectedAsset()
    {
        var selectedAssets = Selection.GetFiltered<Object>(SelectionMode.Assets);
        if (selectedAssets.Length > 0)
        {
            string assetPath = AssetDatabase.GetAssetPath(selectedAssets[0]);
            if (TryGetBundleName(assetPath, out var bundleName))
                Debug.LogFormat("Bundle name <{0}> for asset <{1}>", bundleName, assetPath);
            else
                Debug.LogFormat("Didn't set any bundle to asset <{0}>", assetPath);
        }
    }

    private static bool TryGetBundleName(string assetPath, out string bundleName)
    {
        AssetImporter assetImporter = AssetImporter.GetAtPath(assetPath);
        if (!string.IsNullOrEmpty(assetImporter.assetBundleName))
        {
            bundleName = assetImporter.assetBundleName;
            return true;
        }
        if (assetPath.Equals("Assets"))
        {
            bundleName = null;
            return false;
        }
        string parentPath = Path.GetDirectoryName(assetPath);
        return TryGetBundleName(parentPath, out bundleName);
    }
}
