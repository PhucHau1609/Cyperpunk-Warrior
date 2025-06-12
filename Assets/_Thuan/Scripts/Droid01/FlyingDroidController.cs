using UnityEngine;

public class FlyingDroidController : MonoBehaviour
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

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        target = pointB.position;
        currentHealth = maxHealth;
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

        HealthBarEnemy.Instance?.ShowHealthBar(transform, currentHealth / (float)maxHealth);

        if (currentHealth <= 0)
        {
            animator.SetTrigger("Death");
            currentState = State.Dead;
            rb.linearVelocity = Vector2.zero;
            GetComponent<Collider2D>().enabled = false;
            this.enabled = false;
            HealthBarEnemy.Instance?.HideHealthBar();

            var behavior = GetComponent<BehaviorDesigner.Runtime.BehaviorTree>();
            if (behavior != null) behavior.DisableBehavior();
            Destroy(gameObject, 2f);
        }

        CameraFollow.Instance?.ShakeCamera();
    }
}
