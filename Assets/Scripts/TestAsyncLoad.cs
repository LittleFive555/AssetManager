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
        for (int i = 0; i < LoadTimes; i++)
        {
            int index = i;
            float time1 = Time.realtimeSinceStartup;
            Debug.Log($"Load Start {index}    {time1}");
            StartCoroutine(AssetManager.LoadAssetAsync<GameObject>("Assets/Cube2.prefab", (objCube) =>
            {
                float time2 = Time.realtimeSinceStartup;
                Debug.Log($"Load End {index}    {time2}, offset {time2-time1}");
            }));
        }
    }
}