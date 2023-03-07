using System.IO;

public class AssetConstPath
{
    public const string AssetBundlesDirName = "AssetBundles";
    public const string BundleInfoBundleName = "bundleinfo";
    public const string BundleInfoAssetName = "BundlesInfo";

    public static readonly string BundlePath = Path.Combine("Assets", AssetBundlesDirName);
    public static readonly string BundleInfoPath = Path.Combine(BundlePath, BundleInfoBundleName);
}
