using UnityEngine;

public class BulletBehaviour : MonoBehaviour
{
    public float lifetime = 3f;

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

            Destroy(gameObject);
        }
    }
}
