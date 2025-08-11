using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    public float damageAmount = 20f;
    private bool canDamage = false;

    private float damageCooldown = 0.5f;
    private float nextDamageTime = 0f;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (canDamage && Time.time >= nextDamageTime && collision.CompareTag("Player"))
        {
            var playerHealth = collision.GetComponent<playerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
                nextDamageTime = Time.time + damageCooldown; // tránh trừ liên tục quá nhanh
            }
        }
    }

    // Gọi từ Animation Event
    public void EnableDamage()
    {
        canDamage = true;
        nextDamageTime = Time.time; // reset cooldown để tránh delay đầu
    }

    public void DisableDamage()
    {
        canDamage = false;
    }
}
