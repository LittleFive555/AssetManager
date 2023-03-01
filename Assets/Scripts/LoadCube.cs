using UnityEngine;

public class LoadCube : MonoBehaviour
{
    private GameObject _obj;

    [SerializeField]
    private KeyCode _key;

    // Start is called before the first frame update
    void Start()
    {
        _obj = AssetManager.LoadAsset<GameObject>("Assets/Prefabs/Cube.prefab");
        Instantiate(_obj, transform);
    }

    private void Update()
    {
        if (Input.GetKeyDown(_key))
            AssetManager.UnloadAsset(_obj);
    }
}
