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
            var playerShader = player.GetComponentInChildren<PlayerShader>();
            if (playerShader != null && playerShader.IsInvisible())
            {
                // ✅ Nếu đang đuổi mà player tàng hình → tự nổ
                Debug.Log("💥 EnemyMini nổ vì player tàng hình");
                Explode();
                return;
            }

            Vector3 direction = (player.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;

            if (direction.x > 0)
                transform.localScale = new Vector3(1, 1, 1);
            else if (direction.x < 0)
                transform.localScale = new Vector3(-1, 1, 1);
        }
    }

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
        }
    }

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
            Explode();
        }
    }

    public void Explode()
    {
        if (explosionPrefab != null)
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        if (explosionSoundPrefab != null)
            Instantiate(explosionSoundPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
