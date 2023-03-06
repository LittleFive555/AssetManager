using UnityEngine;

public class LoadCube : MonoBehaviour
{
    private GameObject _obj;

    [SerializeField]
    private KeyCode _key;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
            Load();

        if (Input.GetKeyDown(_key))
            Unload();
    }

    private void Load()
    {
        //_obj = AssetManager.LoadAsset<GameObject>("Assets/Prefabs/Cube.prefab");
        //Instantiate(_obj, transform);

        StartCoroutine(AssetManager.LoadAssetAsync<GameObject>("Assets/Prefabs/Folder/Cube.prefab", (result) =>
        {
            _obj = result;
            Instantiate(_obj, transform);
        }));
    }

    private void Unload()
    {
        AssetManager.UnloadAsset(_obj);
    }
}
