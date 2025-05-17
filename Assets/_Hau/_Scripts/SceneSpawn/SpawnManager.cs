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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        MovePlayerToSpawnPoint();
    }

    private void MovePlayerToSpawnPoint()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        // Tìm tất cả spawn point trong scene
        SpawnPoint[] spawnPoints = FindObjectsOfType<SpawnPoint>();
        foreach (var point in spawnPoints)
        {
            if (point.spawnID == nextSpawnPointID)
            {
                player.transform.position = point.transform.position;
                player.transform.rotation = point.transform.rotation;
                return;
            }
        }

        Debug.LogWarning("SpawnManager: Không tìm thấy spawn point với ID: " + nextSpawnPointID);
    }

    public void SetNextSpawnPoint(string spawnID)
    {
        nextSpawnPointID = spawnID;
    }
}
