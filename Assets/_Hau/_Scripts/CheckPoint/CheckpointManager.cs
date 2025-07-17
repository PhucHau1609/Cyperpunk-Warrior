using UnityEngine;
using UnityEngine.SceneManagement;

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
            player.transform.position = lastCheckpointPosition;
            FinishRespawn(player);
        }
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
            /* if (obj.name.Contains("Main Camera") || obj.name.Contains("UIRoot") || obj.name.Contains("AudioManager"))
             {
                 Destroy(obj);
             }*/
        }
    }
}
