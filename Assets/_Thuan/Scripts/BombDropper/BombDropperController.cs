using UnityEngine;

public class BombDropperController : MonoBehaviour, IDamageResponder
{
    [Header("Combat")]
    public GameObject bombPrefab;
    public Transform dropPoint;
    public float dropInterval = 1f; // 1 giây giữa mỗ lần thả bomb

    [Header("Health")]
    public HealthBarEnemy healthBarEnemy;

    private Transform player;
    private Rigidbody2D rb;
    public Animator animator;
    private EnemyDamageReceiver damageReceiver;
    private ItemDropTable itemDropTable;

    private enum State { Patrolling, Dropping, Dead }
    private State currentState = State.Patrolling;

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        damageReceiver = GetComponent<EnemyDamageReceiver>();
        itemDropTable = GetComponent<ItemDropTable>();

        // Tự động tìm và gán bombPrefab nếu chưa được gán
        if (bombPrefab == null)
        {
            bombPrefab = Resources.Load<GameObject>("Prefabs/Bomb");
        }

        // Tự động tìm dropPoint nếu chưa được gán
        if (dropPoint == null)
        {
            Transform foundDropPoint = transform.Find("DropPoint");
            if (foundDropPoint != null)
            {
                dropPoint = foundDropPoint;
            }
            else
            {
                // Tạo dropPoint tự động ở vị trí dưới enemy
                GameObject dropPointObj = new GameObject("DropPoint");
                dropPointObj.transform.SetParent(transform);
                dropPointObj.transform.localPosition = new Vector3(0, -1f, 0);
                dropPoint = dropPointObj.transform;
            }
        }
    }

    void Start()
    {
        float normalizedHealth = GetNormalizedHealth();
        if (normalizedHealth < 1f)
            healthBarEnemy?.ShowHealthBar(normalizedHealth);
        else
            healthBarEnemy?.HideHealthBar();
    }

    public bool IsPlayerUnderneath()
    {
        if (player == null) return false;

        float xDist = Mathf.Abs(player.position.x - transform.position.x);
        float yDiff = transform.position.y - player.position.y;

        return xDist <= 0.5f && yDiff > 0.5f;
    }

    // Animation Event - được gọi từ animation Attack
    public void OnBombDrop()
    {
        if (bombPrefab != null && dropPoint != null)
        {
            Instantiate(bombPrefab, dropPoint.position, Quaternion.identity);
        }
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

        rb.gravityScale = 1f;
        this.enabled = false;

        healthBarEnemy?.HideHealthBar();

        var behavior = GetComponent<BehaviorDesigner.Runtime.BehaviorTree>();
        if (behavior != null) behavior.DisableBehavior();

        //itemDropTable?.TryDropItems();

        Destroy(gameObject, 2f);
    }

    private float GetNormalizedHealth()
    {
        if (damageReceiver != null && damageReceiver.MaxHP > 0)
            return damageReceiver.CurrentHP / (float)damageReceiver.MaxHP;
        else
            return 1f;
    }
    void DestroySefl()
    {
        Destroy(gameObject);
    }
}