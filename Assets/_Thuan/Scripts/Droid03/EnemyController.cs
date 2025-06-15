using UnityEngine;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour
{
    public float moveSpeed = 2f;
    public Transform pointA;
    public Transform pointB;
    public Transform gunPoint;
    public Animator animator;
    public Slider healthSlider;
    public Transform player;

    [HideInInspector] public Vector3 initialPosition;

    public float maxHealth = 10;
    private float currentHealth;

    private enum State { Patrolling, Chasing, Returning, Dead }
    private State currentState = State.Patrolling;
    private Rigidbody2D rb;

    [SerializeField] private LayerMask groundLayer;

     public HealthBarEnemy healthBarEnemy;

    void Awake()
    {
        if (player == null)
            player = GameObject.FindWithTag("Player")?.transform;
    }

    void Start()
    {
        initialPosition = transform.position;
        currentHealth = maxHealth;
        //UpdateHealthBar();
        rb = GetComponent<Rigidbody2D>();

        float normalizedHealth = currentHealth / (float)maxHealth;
        if (normalizedHealth < 1f && normalizedHealth > 0f)
        {
            healthBarEnemy?.ShowHealthBar(normalizedHealth);
        }
    }

    void FixedUpdate()
    {
        StickToGround(); 
    }

    public void FacePlayer()
    {
        if (player == null) return;

        Vector3 scale = transform.localScale;
        scale.x = (player.position.x < transform.position.x) ? -1 : 1;
        transform.localScale = scale;
    }

    void StickToGround()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 2f, groundLayer);
        if (hit.collider != null)
        {
            Vector3 pos = transform.position;
            pos.y = hit.point.y + 0.1f;
            transform.position = pos;
        }
    }
    
     public void TakeDamage(int damage)
    {
        if (currentState == State.Dead) return;

        currentHealth -= damage;
        animator.SetTrigger("Hurt");

        // Hiển thị thanh máu của Enemy này
        healthBarEnemy?.ShowHealthBar(currentHealth / (float)maxHealth);

        if (currentHealth <= 0)
        {
            animator.SetTrigger("Death");
            currentState = State.Dead;
            rb.linearVelocity = Vector2.zero;
            GetComponent<Collider2D>().enabled = false;
            this.enabled = false;
            
            // Ẩn thanh máu của Enemy này
            healthBarEnemy?.HideHealthBar();

            var behavior = GetComponent<BehaviorDesigner.Runtime.BehaviorTree>();
            if (behavior != null) behavior.DisableBehavior();
            //ItemsDropManager.Instance.DropItem(ItemCode.MachineGun_0, 1, this.transform.position);

            Destroy(gameObject, 2f);
        }

        CameraFollow.Instance?.ShakeCamera();
    }
}
