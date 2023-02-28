using UnityEngine;

public class LoadCube : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var obj = AssetManager.LoadAsset<GameObject>("Assets/Prefabs/Cube.prefab");
        Instantiate(obj);
    }
}
