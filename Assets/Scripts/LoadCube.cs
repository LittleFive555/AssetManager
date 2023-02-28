using UnityEngine;

public class LoadCube : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var obj = AssetManager.LoadAsset<GameObject>("Assets/Prefabs/Cube.prefab");
        Instantiate(obj);
        var bundles = AssetBundle.GetAllLoadedAssetBundles();
        foreach (var bundle in bundles)
            Debug.Log(bundle.name);
    }
}
