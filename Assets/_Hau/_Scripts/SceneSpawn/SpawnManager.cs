using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;

    [HideInInspector]
    public string nextSpawnPointID = "Default";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private bool SceneRequiresSpawn(Scene scene)
    {
        return scene.name != "MapLevel1"; // Hoặc dùng danh sách
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (SceneRequiresSpawn(scene))
            StartCoroutine(DelayedSpawn());
    }

    private IEnumerator DelayedSpawn()
    {
        yield return new WaitForSeconds(2f); // Hoặc WaitForSeconds(0.1f)
        MovePlayerToSpawnPoint();
    }


    private void MovePlayerToSpawnPoint()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        // Nếu không đặt gì trước -> không làm gì cả
        if (string.IsNullOrEmpty(nextSpawnPointID) || nextSpawnPointID == "Default")
            return;

        // Tìm tất cả spawn point trong scene
        SpawnPoint[] spawnPoints = FindObjectsOfType<SpawnPoint>();
        foreach (var point in spawnPoints)
        {
            if (point.sceneName.ToString() == nextSpawnPointID)
            {
                player.transform.position = point.transform.position;
                player.transform.rotation = point.transform.rotation;
                return;
            }
        }

        Debug.LogWarning("SpawnManager: Không tìm thấy spawn point với ID: " + nextSpawnPointID);
    }

    public void SetNextSpawnPoint(SpawnSceneName spawnID)
    {
        nextSpawnPointID = spawnID.ToString();
    }
}
