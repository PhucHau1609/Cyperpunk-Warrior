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
    private ItemDropTable itemDropTable;


    private enum State { Patrolling, Chasing, Returning, Dead }
    private State currentState = State.Patrolling;

    void OnEnable() { PlayerLocator.OnChanged += HandlePlayerChanged; TryResolvePlayer(); }
    void OnDisable() { PlayerLocator.OnChanged -= HandlePlayerChanged; }
    void HandlePlayerChanged(Transform t) { player = t; }
    void TryResolvePlayer() { if (PlayerLocator.Current) player = PlayerLocator.Current; else { var go = GameObject.FindWithTag("Player"); if (go) player = go.transform; } }


    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        damageReceiver = GetComponent<EnemyDamageReceiver>();
        itemDropTable = GetComponent<ItemDropTable>();
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
