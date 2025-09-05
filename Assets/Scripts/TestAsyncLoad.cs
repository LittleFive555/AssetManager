using EdenMeng.AssetManager;
using System;
using UnityEngine;

public class TestAsyncLoad : MonoBehaviour
{
    public int LoadTimes;

    public void Awake()
    {
        AssetManager.InitWithAssetBundle();
    }

    public void Start()
    {
        // TestAsyncLoadAssetMultipleTimes(LoadTimes);
        TestAsyncAndSyncLoadAsset();
        // TestAsyncLoadAndUnloadImmediatelly();
        // TestAsyncAndSyncLoad();
    }

    public void TestAsyncLoadAssetMultipleTimes(int times)
    {
        for (int i = 0; i < LoadTimes; i++)
        {
            int index = i;
            StartCoroutine(AssetManager.LoadAssetAsync<GameObject>("Assets/Cube2.prefab", null));
        }
    }

    public void TestAsyncAndSyncLoadAsset()
    {
        StartCoroutine(AssetManager.LoadAssetAsync<GameObject>("Assets/Cube2.prefab", (gameObject) =>
        {
            AssetManager.UnloadAsset(gameObject);
        }));
        var obj2 = AssetManager.LoadAsset<GameObject>("Assets/Cube2.prefab");
        AssetManager.UnloadAsset(obj2);
    }

    public void TestAsyncLoadAndUnloadBundleImmediatelly()
    {
        StartCoroutine(AssetBundleManager.LoadBundleAndDependenciesAsync("prefab2", null));
        AssetBundleManager.UnloadBundleAndDependencies("prefab2");
        AssetBundleManager.LogAllLoadedBundle();
    }

    public void TestAsyncAndSyncLoadBundle()
    {
        StartCoroutine(AssetBundleManager.LoadBundleAndDependenciesAsync("prefab2", null));
        AssetBundleManager.LoadBundleAndDependencies("prefab2");
    }
}