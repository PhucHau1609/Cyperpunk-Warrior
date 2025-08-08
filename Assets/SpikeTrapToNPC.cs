using UnityEngine;

public class SpikeTrapToNPC : MonoBehaviour
{
    public float damage = 10f;
    public float damageCooldown = 1f;

    private float lastDamageTime = -Mathf.Infinity;
    private Transform currentTarget;

    // Gọi khi có NPC đi vào bẫy, lưu lại mục tiêu
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("NPC"))
        {
            currentTarget = other.transform;
        }
    }

    // Gọi từ Animation Event đúng thời điểm gây sát thương
    public void DealDamageToTarget()
    {
        if (currentTarget == null) return;
        if (Time.time - lastDamageTime < damageCooldown) return;

        LyraHealth lyraHealth = currentTarget.GetComponent<LyraHealth>();
        if (lyraHealth != null)
        {
            lyraHealth.TakeDamage((int)damage);
            lastDamageTime = Time.time;
        }
    }
}
