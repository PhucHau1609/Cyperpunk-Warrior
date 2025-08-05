using UnityEngine;

public class LaserActivator : MonoBehaviour
{
    public LaserManagerTrap laserManager;

    private Collider2D myCollider;

    private void Awake()
    {
        myCollider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            laserManager.ActivateLasers();

            // ❌ KHÔNG tắt object → chỉ vô hiệu hóa collider
            if (myCollider != null)
                myCollider.enabled = false;
        }
    }

    public void ResetTrigger()
    {
        if (myCollider == null)
            myCollider = GetComponent<Collider2D>();

        // ✅ Bật lại trigger khi player hồi sinh
        myCollider.enabled = true;
    }
}
