using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CheckpointManager : HauSingleton<CheckpointManager>
{
    public CheckPointEnum lastCheckpointID;
    public Vector3 lastCheckpointPosition;
    public string lastCheckpointScene;
    
    public void SetCurrentCheckpoint(CheckPointEnum id, Vector3 position, string sceneName)
    {
        lastCheckpointID = id;
        lastCheckpointPosition = position;
        lastCheckpointScene = sceneName;
    }

    public void RespawnPlayer(GameObject player)
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (lastCheckpointScene != currentScene)
        {
            LoadSceneWithCleanup(lastCheckpointScene, player);
        }
        else
        {
            // Reset tất cả bosses trước khi respawn player
            ResetAllBossesInScene();
            
            player.transform.position = lastCheckpointPosition;
            FinishRespawn(player);
        }
    }

    private void ResetAllBossesInScene()
    {
        // Reset tất cả BossPhu trong scene
        BossPhuController[] bossPhuList = FindObjectsByType<BossPhuController>(FindObjectsSortMode.None);
        foreach (var boss in bossPhuList)
        {
            if (boss != null)
            {
                boss.ResetBoss();
            }
        }
        
        // Reset tất cả MiniBoss trong scene
        MiniBoss[] miniBossList = FindObjectsByType<MiniBoss>(FindObjectsSortMode.None);
        foreach (var miniBoss in miniBossList)
        {
            if (miniBoss != null)
            {
                miniBoss.ResetBoss();
            }
        }
        
        // Reset tất cả Boss2 trong scene
        Boss2Controller[] boss2List = FindObjectsByType<Boss2Controller>(FindObjectsSortMode.None);
        foreach (var boss2 in boss2List)
        {
            if (boss2 != null)
            {
                boss2.ResetBoss();
            }
        }
        
        Debug.Log($"Đã reset {bossPhuList.Length} BossPhu, {miniBossList.Length} MiniBoss, {boss2List.Length} Boss2 trong scene");
    }

    private void FinishRespawn(GameObject player)
    {
        CharacterController2D controller = player.GetComponent<CharacterController2D>();
        if (controller != null)
        {
            controller.RestoreFullLife();
        }
    }

    private void LoadSceneWithCleanup(string sceneName, GameObject player)
    {
        // Cleanup các đối tượng không cần giữ lại
        CleanupScene();

        // Sau khi cleanup xong -> Load lại scene chứa checkpoint
        SceneManager.LoadSceneAsync(sceneName).completed += (op) =>
        {
            // Khi scene mới load, bosses sẽ tự động ở trạng thái ban đầu
            player.transform.position = lastCheckpointPosition;
            FinishRespawn(player);
        };
    }

    private void CleanupScene()
    {
        var allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        foreach (var obj in allObjects)
        {
            if (obj.CompareTag("PersistentObject")) continue;
            if (obj == this.gameObject) continue; // Đừng tự xoá chính mình

            // Có thể kiểm tra tên, layer, hoặc component đặc biệt
            if (obj.name.Contains("UIRoot") || obj.name.Contains("AudioManager"))
            {
                Destroy(obj);
            }
        }
    }
}