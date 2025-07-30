using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// ScriptableObject để lưu data tương thích với Unity
[System.Serializable]
public class SerializablePrefabData
{
    public string name;
    public GameObject prefab;
    public bool checkForExisting;
    public string existingCheckType;
    public string existingCheckValue;
}

[CreateAssetMenu(fileName = "QuickTestSetup", menuName = "Tools/Quick Test Setup Data")]
public class QuickTestSetupDataAsset : ScriptableObject
{
    public SerializablePrefabData[] corePrefabs;
    public GameObject[] essentialPrefabs;
    public Vector3 playerSpawnPosition;
    public bool autoFindSpawnPoint;
    public string saveDataPath;

    public void CopyFromSetupData(QuickTestSetupTool.TestSetupData source)
    {
        // Convert PrefabData to SerializablePrefabData
        corePrefabs = new SerializablePrefabData[source.corePrefabs.Count];
        for (int i = 0; i < source.corePrefabs.Count; i++)
        {
            corePrefabs[i] = new SerializablePrefabData
            {
                name = source.corePrefabs[i].name,
                prefab = source.corePrefabs[i].prefab,
                checkForExisting = source.corePrefabs[i].checkForExisting,
                existingCheckType = source.corePrefabs[i].existingCheckType,
                existingCheckValue = source.corePrefabs[i].existingCheckValue
            };
        }

        essentialPrefabs = source.essentialPrefabs.ToArray();
        playerSpawnPosition = source.playerSpawnPosition;
        autoFindSpawnPoint = source.autoFindSpawnPoint;
        saveDataPath = source.saveDataPath;
    }

    public QuickTestSetupTool.TestSetupData ToSetupData()
    {
        var result = new QuickTestSetupTool.TestSetupData();
        result.corePrefabs.Clear();

        // Convert SerializablePrefabData back to PrefabData
        if (corePrefabs != null)
        {
            foreach (var serializable in corePrefabs)
            {
                result.corePrefabs.Add(new QuickTestSetupTool.PrefabData(
                    serializable.name,
                    serializable.prefab,
                    serializable.checkForExisting,
                    serializable.existingCheckType,
                    serializable.existingCheckValue
                ));
            }
        }

        result.essentialPrefabs = essentialPrefabs?.ToList() ?? new List<GameObject>();
        result.playerSpawnPosition = playerSpawnPosition;
        result.autoFindSpawnPoint = autoFindSpawnPoint;
        result.saveDataPath = saveDataPath;

        return result;
    }
}