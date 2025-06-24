using UnityEngine;

public class LaserActivator : MonoBehaviour
{
    public LaserManagerTrap  laserManager;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            laserManager.ActivateLasers();
            gameObject.SetActive(false); // Ẩn trigger sau khi dùng
        }
    }
}
