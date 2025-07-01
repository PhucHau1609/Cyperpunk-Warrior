using UnityEngine;

public class BulletTurret : MonoBehaviour
{
    public float lifetime = 4f;
    public GameObject hitEffectGround;
    public GameObject hitEffectEnemy;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground"))
        {
            if (hitEffectGround != null)
                Instantiate(hitEffectGround, transform.position, Quaternion.identity);

            Destroy(gameObject);
        }

        if (collision.CompareTag("NPC"))
        {
            if (hitEffectEnemy != null)
                Instantiate(hitEffectEnemy, transform.position, Quaternion.identity);

            Destroy(gameObject);
        }
    }
}
