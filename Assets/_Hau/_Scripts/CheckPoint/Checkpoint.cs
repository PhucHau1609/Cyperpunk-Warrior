using UnityEngine;
using UnityEngine.SceneManagement;

public class Checkpoint : MonoBehaviour
{
    public CheckPointEnum checkpointID; // Dễ mở rộng lưu dữ liệu nếu cần
    public Transform spawnPoint;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            CheckpointManager.Instance.SetCurrentCheckpoint(checkpointID, 
                spawnPoint.position, 
                SceneManager.GetActiveScene().name);
        }
    }
}

public enum CheckPointEnum
{
    CheckPoint_0 = 0,
    CheckPoint_1 = 1,
    CheckPoint_2 = 2,
    CheckPoint_3 = 3,
    CheckPoint_4 = 4,
    CheckPoint_5 = 5,
    CheckPoint_6 = 6,
    CheckPoint_7 = 7,
    CheckPoint_8 = 8,
    CheckPoint_9 = 9,
        
    CheckPoint_10 = 10,
    CheckPoint_11 = 11,
    CheckPoint_12 = 12,
    CheckPoint_13 = 13,

}
