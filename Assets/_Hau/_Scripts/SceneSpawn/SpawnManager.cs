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
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;

        //Debug.Log("📂 Log file path: " + Application.persistentDataPath);
        //LogToFile("📂 Log file path: " + Application.persistentDataPath);
    }

    private bool SceneRequiresSpawn(Scene scene)
    {
        return scene.name != "MapLevel1";
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ✅ Nếu có save scene trùng scene hiện tại → dịch chuyển tới SavedPosition
        if (UserSession.Instance != null
            && UserSession.Instance.HasLoadedSave
            && !string.IsNullOrEmpty(UserSession.Instance.SavedSceneName)
            && UserSession.Instance.SavedSceneName == scene.name)
        {
            StartCoroutine(MovePlayerToSavedPositionCo());
            return; // đã xử lý spawn theo save
        }

        // ❇️ Luồng cũ theo spawnPoint ID
        if (SceneRequiresSpawn(scene))
        {
            StartCoroutine(DelayedSpawn());
        }
    }

    private IEnumerator MovePlayerToSavedPositionCo()
    {
        GameObject player = PlayerSpawner.Instance != null ? PlayerSpawner.Instance.EnsurePlayer() : null;
        while (player == null) { player = GameObject.FindGameObjectWithTag("Player"); yield return null; }

        var rb = player.GetComponent<Rigidbody2D>();
        if (rb) { rb.linearVelocity = Vector2.zero; rb.simulated = false; }

        player.transform.position = UserSession.Instance.SavedPosition;

        if (rb) { yield return null; rb.simulated = true; }

        CameraFollow.Instance?.TryFindPlayer();
        UserSession.Instance.HasLoadedSave = false;
    }


    /*    private IEnumerator DelayedSpawn()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            yield return new WaitUntil(() => player != null);

            Scene playerScene = player.scene;
            //LogToFile("✅ Found player. Scene of player: " + playerScene.name);

            if (playerScene.name != "DontDestroyOnLoad")
            {
                Debug.LogWarning("⚠ Có thể player hiện tại không phải từ DontDestroyOnLoad");
                //LogToFile("⚠ Player không phải từ DontDestroyOnLoad, đang ở scene: " + playerScene.name);
                yield break;
            }

            yield return new WaitForEndOfFrame();
            //LogToFile("➡ Moving player to spawn point...");
            MovePlayerToSpawnPoint();

            yield return new WaitForSeconds(0.5f);
            var itemPicker = player.GetComponentInChildren<ItemsPicker>();
            if (itemPicker != null)
            {
                if (itemPicker.mainCamera == null)
                {
                    itemPicker.RefreshCamera();
                    //LogToFile("📷 Refreshed camera for item picker");
                }
            }
        }
    */


    private IEnumerator DelayedSpawn()
    {
        // ✅ luôn đảm bảo có player
        GameObject player = PlayerSpawner.Instance != null ? PlayerSpawner.Instance.EnsurePlayer()
                                                           : GameObject.FindGameObjectWithTag("Player");

        yield return new WaitUntil(() => player != null);

        // ❌ BỎ điều kiện bắt buộc DDOL
        // if (player.scene.name != "DontDestroyOnLoad") { yield break; }

        yield return new WaitForEndOfFrame();
        MovePlayerToSpawnPoint();

        // Đồng bộ camera / item picker
        var itemPicker = player.GetComponentInChildren<ItemsPicker>();
        if (itemPicker && itemPicker.mainCamera == null) itemPicker.RefreshCamera();
    }


    private void MovePlayerToSpawnPoint()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            //LogToFile("❌ Không tìm thấy Player");
            return;
        }

        //LogToFile("➡ Bắt đầu tìm spawn point, nextSpawnPointID = " + nextSpawnPointID);

        if (string.IsNullOrEmpty(nextSpawnPointID) || nextSpawnPointID == "Default")
        {
            //LogToFile("⚠ nextSpawnPointID bị null hoặc default → bỏ qua spawn");
            return;
        }

        SpawnPoint[] spawnPoints = Object.FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
        //LogToFile("🔍 Có tổng cộng " + spawnPoints.Length + " spawn point trong scene");

        foreach (var point in spawnPoints)
        {
            //LogToFile("🧩 So sánh: " + point.sceneName + " == " + nextSpawnPointID);
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

                //LogToFile("✅ Player moved to: " + point.transform.position + " | Rotation: " + point.transform.rotation);

                if (rb != null)
                    StartCoroutine(ReenablePhysics(rb));

                return;
            }
        }

        var allPlayers = GameObject.FindGameObjectsWithTag("Player");
        //LogToFile("🔍 Số lượng Player hiện tại: " + allPlayers.Length);
        foreach (var p in allPlayers)
        {
            //LogToFile("👉 Player: " + p.name + " | Pos: " + p.transform.position + " | Scene: " + p.scene.name);
        }


        //LogToFile("❌ Không tìm thấy spawn point trùng với ID: " + nextSpawnPointID);
    }

    private IEnumerator ReenablePhysics(Rigidbody2D rb)
    {
        yield return null;
        rb.simulated = true;
    }

    public void SetNextSpawnPoint(SpawnSceneName spawnID)
    {
        nextSpawnPointID = spawnID.ToString();
        //LogToFile("✅ SetNextSpawnPoint → " + nextSpawnPointID);
    }

    private void LogToFile(string message)
    {
        string path = Application.persistentDataPath + "/debug_log.txt";
        string finalMessage = System.DateTime.Now.ToString("HH:mm:ss") + " | " + message;
        System.IO.File.AppendAllText(path, finalMessage + "\n");
    }
}
