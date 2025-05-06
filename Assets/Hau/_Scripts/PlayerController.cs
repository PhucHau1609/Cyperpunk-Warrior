using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float rayLength = 0.5f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private float moveInput;
    private bool isGrounded;
    private bool isJumping;

    [Header("Testing")]
    [SerializeField] protected bool isHurt = false;
    [SerializeField] protected bool isDead = false;
    private PlayerAnimationController animationController;
    private bool isAttacking = false;



    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animationController = GetComponent<PlayerAnimationController>();

    }

    private void Update()
    {
        HandleInput();
        UpdateAnimationState();
        FlipSprite();
    }

    private void FixedUpdate()
    {
        Move();
        CheckGround();
    }

    void HandleInput()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            isJumping = true;
        }

        if (Input.GetKeyDown(KeyCode.Z))
            animationController.Trigger("Attack1");
        if (Input.GetKeyDown(KeyCode.X))
            animationController.Trigger("Attack2");
        if (Input.GetKeyDown(KeyCode.C))
            animationController.Trigger("Attack3");
        if (Input.GetKeyDown(KeyCode.V))
            animationController.Trigger("Attack4");
    }

    void Move()
    {
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

        if (isJumping)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            isJumping = false;
        }
    }

    void CheckGround()
    {
        RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, Vector2.down, rayLength, groundLayer);
        isGrounded = hit.collider != null;

        Debug.DrawRay(groundCheck.position, Vector2.down * rayLength, isGrounded ? Color.green : Color.red);
    }


    void FlipSprite()
    {
        if (moveInput > 0)
            spriteRenderer.flipX = false;
        else if (moveInput < 0)
            spriteRenderer.flipX = true;

    }

    void UpdateAnimationState()
    {
        animationController.SetFloat("Speed", Mathf.Abs(moveInput));
        animationController.SetFloat("VerticalVelocity", rb.velocity.y);
        animationController.SetBool("isGrounded", isGrounded);

    }
}


/*  if (isDead)
  {
      animationController.Trigger("Death");
      return;
  }

  if (isHurt)
  {
      animationController.Trigger("Hurt");
      return;
  }*/

