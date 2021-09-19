using UnityEngine;

[System.Serializable]
public class AltCostume
{
    public string name;
    public StorageMode characterPrefabStorage = StorageMode.Prefab;
    public GameObject prefab;
    public string prefabResourcePath;
    public bool enableColorMask;
    public Color colorMask;
}