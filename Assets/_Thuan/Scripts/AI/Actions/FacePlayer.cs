using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FacePlayer : Action
{
    public SharedTransform player;
    private Transform cachedPlayerTransform;

    public override void OnStart()
    {
        // Nếu chưa có player, tìm kiếm
        if (player == null || player.Value == null)
        {
            // Thử tìm player từ cached
            if (cachedPlayerTransform == null)
            {
                cachedPlayerTransform = FindPlayerInScene();
            }

            // Gán giá trị
            if (cachedPlayerTransform != null)
            {
                player.Value = cachedPlayerTransform;
            }
            else
            {
                // Đăng ký sự kiện để tìm player khi scene load
                SceneManager.sceneLoaded += OnSceneLoaded;
                Debug.LogWarning("[FacePlayer] Không tìm thấy Player");
            }
        }
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

    public override TaskStatus OnUpdate()
    {
        // Kiểm tra player
        if (player.Value == null)
            return TaskStatus.Failure;

        // Tính toán hướng
        Vector2 dir = player.Value.position - transform.position;

        // Thay đổi hướng
        if (dir.x != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Sign(dir.x) * Mathf.Abs(scale.x);
            transform.localScale = scale;
        }

        return TaskStatus.Success;
    }
}