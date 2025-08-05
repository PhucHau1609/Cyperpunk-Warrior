using UnityEngine;
using UnityEngine.UI;

public class SpiderEnemyController : MonoBehaviour, IDamageResponder
{
    public float moveSpeed = 2f;
    public Transform pointA;
    public Transform pointB;
    public Transform gunPoint;
    public Animator animator;
    public Slider healthSlider;
    public Transform player;

    [HideInInspector] public Vector3 initialPosition;

    private enum State { Patrolling, Chasing, Returning, Dead }
    private State currentState = State.Patrolling;

    private Rigidbody2D rb;

    public HealthBarEnemy healthBarEnemy;
    private EnemyDamageReceiver damageReceiver;
    private ItemDropTable itemDropTable;


    void Awake()
    {
        if (player == null)
            player = GameObject.FindWithTag("Player")?.transform;

        damageReceiver = GetComponent<EnemyDamageReceiver>();
        rb = GetComponent<Rigidbody2D>();
        itemDropTable = GetComponent<ItemDropTable>();;
    }

    void Start()
    {
        initialPosition = transform.position;

        // Ẩn HealthBar lúc bắt đầu (nếu đầy máu)
        float normalizedHealth = GetNormalizedHealth();
        if (healthBarEnemy != null)
        {
            if (normalizedHealth < 1f)
                healthBarEnemy.ShowHealthBar(normalizedHealth);
            else
                healthBarEnemy.HideHealthBar();
        }
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

        if (healthBarEnemy != null)
        {
            healthBarEnemy.ShowHealthBar(GetNormalizedHealth());
        }
    }

    public void OnDead()
    {
        animator.SetTrigger("Death");
        currentState = State.Dead;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;
        rb.angularVelocity = 0f;
        GetComponent<Collider2D>().enabled = false;
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
}
