#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

public class DatabaseAssetLoader : IAssetLoader
{
    public T LoadAsset<T>(string path) where T : Object
    {
        return AssetDatabase.LoadAssetAtPath<T>(path);
    }

    public T LoadAssetAsync<T>(string path) where T : Object
    {
        return LoadAsset<T>(path);
    }

    public void UnloadAsset<T>(T asset) where T : Object
    {

    }
}

#endif