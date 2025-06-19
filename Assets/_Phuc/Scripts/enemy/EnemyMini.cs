using UnityEngine;

public class EnemyMini : MonoBehaviour, IExplodable
{
    public enum State { Sleep, Awaken, Chase }
    private State currentState = State.Sleep;

    public float speed = 5f;
    public GameObject explosionPrefab;

    private Transform player;
    private Animator animator;

    public GameObject explosionSoundPrefab;
    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (currentState == State.Chase && player != null)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;
            if (direction.x > 0)
                transform.localScale = new Vector3(1, 1, 1); // hướng phải
            else if (direction.x < 0)
                transform.localScale = new Vector3(-1, 1, 1); // hướng trái

        }
    }

    // Gọi từ LightDetector khi bị phát hiện
    public void Activate(Transform targetPlayer)
    {
        if (currentState == State.Sleep)
        {
            player = targetPlayer;
            currentState = State.Awaken;

            if (animator != null)
            {
                animator.SetTrigger("Awaken");
            }

            // Chờ animation gọi OnAwakenComplete()
        }
    }

    // Gọi từ animation event cuối clip "Awaken"
    public void OnAwakenComplete()
    {
        currentState = State.Chase;

        if (animator != null)
        {
            animator.SetTrigger("Chase");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (currentState == State.Chase && other.CompareTag("Player"))
        {
            Explode(); // Gọi hiệu ứng nổ
        }
    }

    public void Explode()
    {
        // Gọi hiệu ứng nổ (nếu có)
        if (explosionPrefab != null)
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        // Gọi prefab phát âm thanh
        if (explosionSoundPrefab != null)
            Instantiate(explosionSoundPrefab, transform.position, Quaternion.identity);

        // Hủy enemy
        Destroy(gameObject);
    }

}
