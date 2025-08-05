using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FacePlayer : Action
{
    public SharedTransform player;
    private Transform cachedPlayerTransform;
    private PlayerShader playerShader; // Thêm reference

    public override void OnStart()
    {
        if (player == null || player.Value == null)
        {
            if (cachedPlayerTransform == null)
            {
                cachedPlayerTransform = FindPlayerInScene();
            }

            if (cachedPlayerTransform != null)
            {
                player.Value = cachedPlayerTransform;
                playerShader = cachedPlayerTransform.GetComponentInChildren<PlayerShader>(); // Lấy PlayerShader
            }
        }
    }

    public override TaskStatus OnUpdate()
    {
        if (player.Value == null)
            return TaskStatus.Failure;

        // Không quay mặt khi Player tàng hình
        if (playerShader != null && playerShader.IsInvisible())
        {
            return TaskStatus.Failure; // Hoặc Success tùy logic của bạn
        }

        Vector2 dir = player.Value.position - transform.position;

        if (dir.x != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Sign(dir.x) * Mathf.Abs(scale.x);
            transform.localScale = scale;
        }

        return TaskStatus.Success;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        cachedPlayerTransform = FindPlayerInScene();
        if (cachedPlayerTransform != null)
        {
            player.Value = cachedPlayerTransform;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private Transform FindPlayerInScene()
    {
        // Nhiều phương thức tìm kiếm Player
        GameObject playerObject = null;

        // Phương thức 1: Tìm theo tag
        playerObject = GameObject.FindGameObjectWithTag("Player");

        // Phương thức 2: Nếu không tìm thấy, tìm theo tên
        if (playerObject == null)
        {
            playerObject = GameObject.Find("Player");
        }

        return playerObject?.transform;
    }
}