using UnityEngine;

public class DestructibleLyra : MonoBehaviour
{
    public int hitPoints = 4;
    public GameObject explosionEffect;

    private bool isDead = false;

    public void TakeDamage()
    {
        if (isDead) return;

        hitPoints--;

        if (hitPoints <= 0)
        {
            isDead = true;

            // Tắt hành động
            MonoBehaviour[] allActions = GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour script in allActions)
            {
                if (script != this) // đừng tắt chính Destructible
                {
                    script.enabled = false;
                }
            }

            // Gọi hiệu ứng nổ
            Explode();

            // Tuỳ chọn: destroy nếu muốn biến mất
            Destroy(gameObject, 0.3f);
        }
    }

    void Explode()
    {
        if (explosionEffect != null)
        {
            GameObject effect = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }
    }
}
