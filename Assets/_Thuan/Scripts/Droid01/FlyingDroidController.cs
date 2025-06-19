using UnityEngine;
public class FlyingDroidController : MonoBehaviour, IDamageResponder
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public Transform pointA;
    public Transform pointB;

    [Header("Combat")]
    public Transform gunPoint;
    public Transform player;

    [Header("Animation")]
    public Animator animator;

    private Vector3 target;
    private Rigidbody2D rb;
    private bool isMoving = true;

    private enum State { Patrolling, Chasing, Returning, Dead }
    private State currentState = State.Patrolling;

    [Header("Health")]
    public HealthBarEnemy healthBarEnemy;
    private EnemyDamageReceiver damageReceiver;
    private ItemDropTable itemDropTable;


    void Awake()
    {
        if (player == null)
            player = GameObject.FindWithTag("Player")?.transform;

        rb = GetComponent<Rigidbody2D>();
        damageReceiver = GetComponent<EnemyDamageReceiver>();
        itemDropTable = GetComponent<ItemDropTable>();

    }

    void Start()
    {
        target = pointB.position;

        float normalizedHealth = GetNormalizedHealth();
        if (healthBarEnemy != null)
        {
            if (normalizedHealth < 1f)
                healthBarEnemy.ShowHealthBar(normalizedHealth);
            else
                healthBarEnemy.HideHealthBar();
        }
    }

    void FixedUpdate()
    {
        if (isMoving && currentState != State.Dead)
        {
            MoveToTarget();
        }
    }

    void MoveToTarget()
    {
        if (Vector2.Distance(transform.position, target) < 0.2f)
        {
            target = (target == pointA.position) ? pointB.position : pointA.position;
        }

        Vector2 dir = (target - transform.position).normalized;
        rb.linearVelocity = dir * moveSpeed;

        // Flip mặt
        if (dir.x != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Sign(dir.x);
            transform.localScale = scale;
        }

        // animator?.SetBool("Fly", true);
    }

    public void Stop()
    {
        isMoving = false;
        rb.linearVelocity = Vector2.zero;
        // animator?.SetBool("Fly", false);
    }

    public void Resume()
    {
        isMoving = true;
        // animator?.SetBool("Fly", true);
    }

    public void FacePlayer()
    {
        if (player == null) return;

        Vector3 scale = transform.localScale;
        scale.x = (player.position.x < transform.position.x) ? -1 : 1;
        transform.localScale = scale;
    }

    public void OnHurt()
    {
        if (currentState == State.Dead) return;

        animator.SetTrigger("Hurt");
        CameraFollow.Instance?.ShakeCamera();

        float normalizedHealth = GetNormalizedHealth();
        healthBarEnemy?.ShowHealthBar(normalizedHealth);
    }

    public void OnDead()
    {
        animator.SetTrigger("Death");
        currentState = State.Dead;

        rb.linearVelocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;

        healthBarEnemy?.HideHealthBar();

        var behavior = GetComponent<BehaviorDesigner.Runtime.BehaviorTree>();
        if (behavior != null) behavior.DisableBehavior();
        itemDropTable?.TryDropItems();

        Destroy(gameObject, 2f);
    }

    private float GetNormalizedHealth()
    {
        if (damageReceiver != null && damageReceiver.MaxHP > 0)
            return damageReceiver.CurrentHP / (float)damageReceiver.MaxHP;
        else
            return 1f;
    }
}


/*public class FlyingDroidController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public Transform pointA;
    public Transform pointB;

    [Header("Combat")]
    public Transform gunPoint;
    public Transform player;

    [Header("Animation")]
    public Animator animator;

    private Vector3 target;
    private Rigidbody2D rb;
    private bool isMoving = true;

    public float maxHealth = 10;
    private float currentHealth;

    private enum State { Patrolling, Chasing, Returning, Dead }
    private State currentState = State.Patrolling;

     public HealthBarEnemy healthBarEnemy;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        target = pointB.position;
        currentHealth = maxHealth;

        // float normalizedHealth = currentHealth / (float)maxHealth;
        // if (normalizedHealth < 1f && normalizedHealth > 0f)
        // {
        //     healthBarEnemy?.ShowHealthBar(normalizedHealth);
        // }
    }

    void FixedUpdate()
    {
        if (isMoving)
            MoveToTarget();
    }

    void MoveToTarget()
    {
        if (Vector2.Distance(transform.position, target) < 0.2f)
        {
            target = (target == pointA.position) ? pointB.position : pointA.position;
        }

        Vector2 dir = (target - transform.position).normalized;
        rb.linearVelocity = dir * moveSpeed;

        // Flip mặt
        if (dir.x != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Sign(dir.x);
            transform.localScale = scale;
        }

        //animator?.SetBool("Fly", true);
    }

    public void Stop()
    {
        isMoving = false;
        rb.linearVelocity = Vector2.zero;
        //animator?.SetBool("Fly", false);
    }

    public void Resume()
    {
        isMoving = true;
        //animator?.SetBool("Fly", true);
    }

    public void FacePlayer()
    {
        if (player == null) return;

        Vector3 scale = transform.localScale;
        scale.x = (player.position.x < transform.position.x) ? -1 : 1;
        transform.localScale = scale;
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
            Destroy(gameObject, 2f);
        }

        CameraFollow.Instance?.ShakeCamera();
    }
}*/
