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
            Destroy(gameObject);
        }
    }
}
