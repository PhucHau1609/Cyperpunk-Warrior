using UnityEngine;

public class DamageToNPC_01 : MonoBehaviour
{
    public float damage = 10f;                 // Lượng damage mỗi lần
    public float damageCooldown = 1f;          // Thời gian chờ giữa các lần gây damage (giây)

    private float lastDamageTime = -Mathf.Infinity;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("NPC"))
        {
            if (Time.time - lastDamageTime >= damageCooldown)
            {
                LyraHealth lyraHealth = other.GetComponent<LyraHealth>();
                if (lyraHealth != null)
                {
                    lyraHealth.TakeDamage((int)damage);
                    lastDamageTime = Time.time;
                }
            }
        }
    }
}
