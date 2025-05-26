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
        yield return new WaitUntil(() => GameObject.FindGameObjectWithTag("Player") != null);
        yield return new WaitForEndOfFrame();

        MovePlayerToSpawnPoint();
    }


    private void MovePlayerToSpawnPoint()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        if (string.IsNullOrEmpty(nextSpawnPointID) || nextSpawnPointID == "Default")
            return;

        SpawnPoint[] spawnPoints = Object.FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
        foreach (var point in spawnPoints)
        {
            if (point.sceneName.ToString() == nextSpawnPointID)
            {
                var rb = player.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.simulated = false;
                }

                player.transform.position = point.transform.position;
                player.transform.rotation = point.transform.rotation;

                if (rb != null)
                    StartCoroutine(ReenablePhysics(rb));

                return;
            }
        }

    }

    private IEnumerator ReenablePhysics(Rigidbody2D rb)
    {
        yield return null;
        rb.simulated = true;
    }


    public void SetNextSpawnPoint(SpawnSceneName spawnID)
    {
        nextSpawnPointID = spawnID.ToString();
    }
}
