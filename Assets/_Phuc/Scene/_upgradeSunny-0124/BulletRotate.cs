using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BulletRotate : MonoBehaviour
{
    public float lifetime = 4f;
    public GameObject hitEffectGround;
    public GameObject hitEffectEnemy;

    [Header("Âm thanh va chạm")]
    public AudioClip hitSoundGround;
    public AudioClip hitSoundEnemy;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        RotateToVelocity();
    }

    void RotateToVelocity()
    {
        if (rb == null) return;

        Vector2 dir = rb.linearVelocity;

        if (dir.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90f;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground"))
        {
            if (hitEffectGround != null)
                Instantiate(hitEffectGround, transform.position, Quaternion.identity);

            if (hitSoundGround != null)
                AudioSource.PlayClipAtPoint(hitSoundGround, transform.position);

            Destroy(gameObject);
        }

        if (collision.CompareTag("NPC"))
        {
            if (hitEffectEnemy != null)
                Instantiate(hitEffectEnemy, transform.position, Quaternion.identity);

            if (hitSoundEnemy != null)
                AudioSource.PlayClipAtPoint(hitSoundEnemy, transform.position);

            Destroy(gameObject);
        }
    }
}
