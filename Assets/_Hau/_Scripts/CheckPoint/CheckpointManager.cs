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
            // ⚠️ Scene khác -> load lại scene lưu checkpoint
            SceneManager.LoadSceneAsync(lastCheckpointScene).completed += (op) =>
            {
                player.transform.position = lastCheckpointPosition;
                FinishRespawn(player);
            };
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
}
