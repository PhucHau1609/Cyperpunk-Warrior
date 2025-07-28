using UnityEngine;

public class EnableOnTrigger : MonoBehaviour
{
    [SerializeField] GameObject targetToEnable;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (targetToEnable != null)
            {
                // Tắt trigger nếu có Collider2D
                var collider = targetToEnable.GetComponent<Collider2D>();
                if (collider != null) collider.isTrigger = false;

                // Gọi animation "close" nếu có Animator
                var animator = targetToEnable.GetComponent<Animator>();
                if (animator != null) animator.SetTrigger("close");
            }
        }
    }
}
