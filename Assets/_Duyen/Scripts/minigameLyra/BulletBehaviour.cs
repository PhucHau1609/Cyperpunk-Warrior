using UnityEngine;

public class BulletBehaviour : MonoBehaviour
{
    public float lifetime = 3f;

    public LayerMask destroyOnHitLayers; // Gán layer "Enemy" và "Ground" trong Inspector

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("NPC")) // Không tự bắn vào mình
        {
            if (other.CompareTag("Enemy") || other.CompareTag("Gun"))
            {
                DestructibleLyra destructible = other.GetComponent<DestructibleLyra>();
                if (destructible != null)
                {
                    destructible.TakeDamage();
                }
            }

            // Hủy đạn nếu layer thuộc Enemy hoặc Ground
            if (((1 << other.gameObject.layer) & destroyOnHitLayers) != 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
