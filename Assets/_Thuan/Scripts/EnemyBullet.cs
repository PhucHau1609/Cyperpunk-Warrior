using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float damage = 10f;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<playerHealth>()?.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
