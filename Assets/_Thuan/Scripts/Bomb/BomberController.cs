using UnityEngine;

public class BomberController : MonoBehaviour, IDamageResponder
{
    [Header("Combat")]
    public GameObject bombPrefab;
    public Transform dropPoint;
    public float dropInterval = 0.3f;
    public int bombCount = 5;

    [Header("Health")]
    public HealthBarEnemy healthBarEnemy;

    private float lastDropTime;
    private int bombsLeft;

    private Transform player;
    private Rigidbody2D rb;
    private Animator animator;
    private EnemyDamageReceiver damageReceiver;

    private enum State { Patrolling, Chasing, Returning, Dead }
    private State currentState = State.Patrolling;

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        damageReceiver = GetComponent<EnemyDamageReceiver>();
    }

    void Start()
    {
        float normalizedHealth = GetNormalizedHealth();
        if (normalizedHealth < 1f)
            healthBarEnemy?.ShowHealthBar(normalizedHealth);
        else
            healthBarEnemy?.HideHealthBar();
    }

    void Update()
    {
        if (currentState == State.Dead) return;

        if (IsPlayerUnder() && bombsLeft <= 0)
        {
            bombsLeft = bombCount;
            lastDropTime = Time.time - dropInterval;
        }

        if (bombsLeft > 0 && Time.time >= lastDropTime + dropInterval)
        {
            DropBomb();
            bombsLeft--;
            lastDropTime = Time.time;
        }
    }

    bool IsPlayerUnder()
    {
        if (player == null) return false;

        float xDist = Mathf.Abs(player.position.x - transform.position.x);
        float yDiff = transform.position.y - player.position.y;

        return xDist < 1.5f && yDiff > 0.5f;
    }

    void DropBomb()
    {
        Instantiate(bombPrefab, dropPoint.position, Quaternion.identity);
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


/*public class BomberController : MonoBehaviour
{
    public GameObject bombPrefab;
    public Transform dropPoint;
    public float dropInterval = 0.3f;
    public int bombCount = 5;
    private Transform player;

    private float lastDropTime;
    private int bombsLeft;

    public float maxHealth = 10;
    private float currentHealth;

    private enum State { Patrolling, Chasing, Returning, Dead }
    private State currentState = State.Patrolling;

    private Rigidbody2D rb;
    private Animator animator;

     public HealthBarEnemy healthBarEnemy;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        currentHealth = maxHealth;

        float normalizedHealth = currentHealth / (float)maxHealth;
        if (normalizedHealth < 1f && normalizedHealth > 0f)
        {
            healthBarEnemy?.ShowHealthBar(normalizedHealth);
        }
    }

    void Update()
    {
        if (IsPlayerUnder() && bombsLeft <= 0)
        {
            bombsLeft = bombCount;
            lastDropTime = Time.time - dropInterval;
        }

        if (bombsLeft > 0 && Time.time >= lastDropTime + dropInterval)
        {
            DropBomb();
            bombsLeft--;
            lastDropTime = Time.time;
        }
    }

    bool IsPlayerUnder()
    {
        if (player == null) return false;

        float xDist = Mathf.Abs(player.position.x - transform.position.x);
        float yDiff = transform.position.y - player.position.y;

        return xDist < 1.5f && yDiff > 0.5f;
    }

    void DropBomb()
    {
        Instantiate(bombPrefab, dropPoint.position, Quaternion.identity);
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
}
*/