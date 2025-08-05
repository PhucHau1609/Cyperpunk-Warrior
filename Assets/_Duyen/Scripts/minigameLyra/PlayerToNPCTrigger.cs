using UnityEngine;

public class PlayerToNPCTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SceneController controller = FindFirstObjectByType<SceneController>();
            if (controller != null)
            {
                controller.SwitchToPetControl();
                //gameObject.SetActive(false); // tắt trigger nếu chỉ dùng 1 lần
            }
        }
    }
}
