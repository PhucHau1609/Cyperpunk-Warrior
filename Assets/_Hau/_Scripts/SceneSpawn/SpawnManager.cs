using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

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
        //DontDestroyOnLoad(gameObject);
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


        GameObject player = GameObject.FindGameObjectWithTag("Player");

        // Chờ tới khi Player từ DontDestroyOnLoad xuất hiện
        yield return new WaitUntil(() => player != null);

        // Tránh bug nếu scene có sẵn player khác
        Scene playerScene = player.scene;
        if (playerScene.name != "DontDestroyOnLoad")
        {
            Debug.LogWarning("Có thể player hiện tại không phải từ DontDestroyOnLoad");
            LogToFile("⚠ Player không phải từ DontDestroyOnLoad, đang ở scene: " + playerScene.name);
            yield break;
        }

        yield return new WaitForEndOfFrame();
        MovePlayerToSpawnPoint();


        // ✅ Gọi RefreshCamera sau khi move xong
        yield return new WaitForSeconds(0.5f); // delay 1 chút nếu cần
        var itemPicker = player.GetComponentInChildren<ItemsPicker>();
        if (itemPicker != null)
        {
            if (itemPicker.mainCamera == null)
            {
                itemPicker.RefreshCamera();
                //Debug.Log("Camera refreshed from SpawnManager");

            }
        }
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
                //Debug.Log("Player hiện đang ở scene: " + player.scene.name);

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

    private void LogToFile(string message)
    {
        string path = Application.persistentDataPath + "/debug_log.txt";
        System.IO.File.AppendAllText(path, Time.time + " | " + message + "\n");
    }

}
