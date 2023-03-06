#if UNITY_EDITOR

using System.Collections;
using UnityEditor;
using UnityEngine;

public class DatabaseAssetLoader : IAssetLoader
{
    public T LoadAsset<T>(string path) where T : Object
    {
        return AssetDatabase.LoadAssetAtPath<T>(path);
    }

    public IEnumerator LoadAssetAsync<T>(string path, System.Action<T> onComplete) where T : Object
    {
        onComplete?.Invoke(LoadAsset<T>(path));
        yield break;
    }

    public void UnloadAsset<T>(T asset) where T : Object
    {

    }
}

#endif