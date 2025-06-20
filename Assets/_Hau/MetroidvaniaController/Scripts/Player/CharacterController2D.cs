using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System.Collections;
using com.cyborgAssets.inspectorButtonPro;

public class CharacterController2D : MonoBehaviour
{
    [Header("Cài đặt di chuyển")]
    [SerializeField] private float m_JumpForce = 400f;
    [Range(0, .3f)][SerializeField] private float m_MovementSmoothing = .05f;
    [SerializeField] private bool m_AirControl = false;
    [SerializeField] private LayerMask m_WhatIsGround;
    [SerializeField] private Transform m_GroundCheck;
    [SerializeField] private Transform m_WallCheck;

    [Header("Cài đặt máu")]
    public float life = 10f;
    public float maxLife = 10f;

    [Header("UI Thanh Máu")]
    public Image healthBar;

    [Header("Hiệu ứng hồi máu")]
    public GameObject healEffect;

    const float k_GroundedRadius = .2f;
    private bool m_Grounded;
    private Rigidbody2D m_Rigidbody2D;
    private bool m_FacingRight = true;
    private Vector3 velocity = Vector3.zero;
    private float limitFallSpeed = 25f;

    public bool canDoubleJump = true;
    [SerializeField] private float m_DashForce = 25f;
    private bool canDash = true;
    private bool isDashing = false;
    private bool m_IsWall = false;
    private bool isWallSliding = false;
    private bool oldWallSlidding = false;
    private float prevVelocityX = 0f;
    private bool canCheck = false;

    private PlayerGravityController gravityController;

    public bool invincible = false;
    private bool canMove = true;

    private Animator animator;
    public ParticleSystem particleJumpUp;
    public ParticleSystem particleJumpDown;

    private float jumpWallStartX = 0;
    private float jumpWallDistX = 0;
    private bool limitVelOnWallJump = false;

    [Header("Events")]
    public UnityEvent OnFallEvent;
    public UnityEvent OnLandEvent;

    private void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        gravityController = GetComponent<PlayerGravityController>();

        if (OnFallEvent == null)
            OnFallEvent = new UnityEvent();
        if (OnLandEvent == null)
            OnLandEvent = new UnityEvent();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        if (maxLife <= 0) maxLife = life;

        if (healEffect != null)
            healEffect.SetActive(false);

        if (healthBar == null)
        {
            GameObject found = GameObject.FindWithTag("HealthBar");
            if (found != null)
                healthBar = found.GetComponent<Image>();
        }
    }

    private void FixedUpdate()
    {
        bool wasGrounded = m_Grounded;
        m_Grounded = false;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
                m_Grounded = true;
            if (!wasGrounded)
            {
                OnLandEvent.Invoke();
                if (!m_IsWall && !isDashing)
                    particleJumpDown.Play();
                canDoubleJump = true;
                if (m_Rigidbody2D.linearVelocity.y < 0f)
                    limitVelOnWallJump = false;
            }
        }

        m_IsWall = false;

        if (!m_Grounded)
        {
            OnFallEvent.Invoke();
            Collider2D[] collidersWall = Physics2D.OverlapCircleAll(m_WallCheck.position, k_GroundedRadius, m_WhatIsGround);
            for (int i = 0; i < collidersWall.Length; i++)
            {
                if (collidersWall[i].gameObject != null)
                {
                    isDashing = false;
                    m_IsWall = true;
                }
            }
            prevVelocityX = m_Rigidbody2D.linearVelocity.x;
        }

        if (limitVelOnWallJump)
        {
            if (m_Rigidbody2D.linearVelocity.y < -0.5f)
                limitVelOnWallJump = false;

            jumpWallDistX = (jumpWallStartX - transform.position.x) * transform.localScale.x;

            if (jumpWallDistX < -0.5f && jumpWallDistX > -1f)
                canMove = true;
            else if (jumpWallDistX < -1f && jumpWallDistX >= -2f)
            {
                canMove = true;
                m_Rigidbody2D.linearVelocity = new Vector2(10f * transform.localScale.x, m_Rigidbody2D.linearVelocity.y);
            }
            else
            {
                limitVelOnWallJump = false;
                m_Rigidbody2D.linearVelocity = new Vector2(0, m_Rigidbody2D.linearVelocity.y);
            }
        }

        // Cập nhật thanh máu mỗi frame
        if (healthBar != null)
            healthBar.fillAmount = Mathf.Clamp01(life / maxLife);
    }

    public void Move(float move, bool jump, bool dash)
    {
        float jumpDirection = gravityController != null && gravityController.IsGravityInverted() ? -1f : 1f;
        if (!canMove) return;

        if (dash && canDash && !isWallSliding)
            StartCoroutine(DashCooldown());

        if (isDashing)
        {
            m_Rigidbody2D.linearVelocity = new Vector2(transform.localScale.x * m_DashForce, 0);
        }
        else if (m_Grounded || m_AirControl)
        {
            if (m_Rigidbody2D.linearVelocity.y < -limitFallSpeed)
                m_Rigidbody2D.linearVelocity = new Vector2(m_Rigidbody2D.linearVelocity.x, -limitFallSpeed);

            Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.linearVelocity.y);
            m_Rigidbody2D.linearVelocity = Vector3.SmoothDamp(m_Rigidbody2D.linearVelocity, targetVelocity, ref velocity, m_MovementSmoothing);

            if (move > 0 && !m_FacingRight && !isWallSliding) Flip();
            else if (move < 0 && m_FacingRight && !isWallSliding) Flip();
        }

        if (m_Grounded && jump)
        {
            animator.SetBool("IsJumping", true);
            animator.SetBool("JumpUp", true);
            m_Grounded = false;
            m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce * jumpDirection));
            canDoubleJump = true;
            particleJumpDown.Play();
            particleJumpUp.Play();
        }
        else if (!m_Grounded && jump && canDoubleJump && !isWallSliding)
        {
            canDoubleJump = false;
            m_Rigidbody2D.linearVelocity = new Vector2(m_Rigidbody2D.linearVelocity.x, 0);
            m_Rigidbody2D.AddForce(new Vector2(0f, (m_JumpForce / 1.2f) * jumpDirection));
            animator.SetBool("IsDoubleJumping", true);
        }

        else if (m_IsWall && !m_Grounded)
        {
            if (!oldWallSlidding && m_Rigidbody2D.linearVelocity.y < 0 || isDashing)
            {
                isWallSliding = true;
                m_WallCheck.localPosition = new Vector3(-m_WallCheck.localPosition.x, m_WallCheck.localPosition.y, 0);
                Flip();
                StartCoroutine(WaitToCheck(0.1f));
                canDoubleJump = true;
                animator.SetBool("IsWallSliding", true);
            }

            isDashing = false;

            if (isWallSliding)
            {
                if (move * transform.localScale.x > 0.1f)
                    StartCoroutine(WaitToEndSliding());
                else
                {
                    oldWallSlidding = true;
                    m_Rigidbody2D.linearVelocity = new Vector2(-transform.localScale.x * 2, -5);
                }
            }

            if (jump && isWallSliding)
            {
                animator.SetBool("IsJumping", true);
                animator.SetBool("JumpUp", true);
                m_Rigidbody2D.linearVelocity = Vector2.zero;
                m_Rigidbody2D.AddForce(new Vector2(transform.localScale.x * m_JumpForce * 1.2f, m_JumpForce * jumpDirection));
                jumpWallStartX = transform.position.x;
                limitVelOnWallJump = true;
                canDoubleJump = true;
                isWallSliding = false;
                animator.SetBool("IsWallSliding", false);
                oldWallSlidding = false;
                m_WallCheck.localPosition = new Vector3(Mathf.Abs(m_WallCheck.localPosition.x), m_WallCheck.localPosition.y, 0);
                canMove = false;
            }
            else if (dash && canDash)
            {
                isWallSliding = false;
                animator.SetBool("IsWallSliding", false);
                oldWallSlidding = false;
                m_WallCheck.localPosition = new Vector3(Mathf.Abs(m_WallCheck.localPosition.x), m_WallCheck.localPosition.y, 0);
                canDoubleJump = true;
                StartCoroutine(DashCooldown());
            }
        }
        else if (isWallSliding && !m_IsWall && canCheck)
        {
            isWallSliding = false;
            animator.SetBool("IsWallSliding", false);
            oldWallSlidding = false;
            m_WallCheck.localPosition = new Vector3(Mathf.Abs(m_WallCheck.localPosition.x), m_WallCheck.localPosition.y, 0);
            canDoubleJump = true;
        }
    }

    public void Heal(float amount)
    {
        life = Mathf.Clamp(life + amount, 0f, maxLife);
        PlayHealEffect();
    }

    public void PlayHealEffect()
    {
        if (healEffect == null) return;

        StopAllCoroutines();
        StartCoroutine(HealEffectRoutine());
    }

    private IEnumerator HealEffectRoutine()
    {
        healEffect.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        healEffect.SetActive(false);
    }

    public void Flip()
    {
        m_FacingRight = !m_FacingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    [ProButton]
    public void ApplyDamage(float damage, Vector3 position)
    {
        if (!invincible)
        {
            animator.SetBool("Hit", true);
            CameraFollow.Instance.ShakeCamera();
            life -= damage;
            Vector2 damageDir = Vector3.Normalize(transform.position - position) * 40f;
            m_Rigidbody2D.linearVelocity = Vector2.zero;

            if (life <= 0)
                StartCoroutine(WaitToDead());
            else
            {
                StartCoroutine(Stun(0.25f));
                StartCoroutine(MakeInvincible(1f));
            }
        }
    }

    IEnumerator DashCooldown()
    {
        animator.SetBool("IsDashing", true);
        isDashing = true;
        canDash = false;
        yield return new WaitForSeconds(0.1f);
        isDashing = false;
        yield return new WaitForSeconds(0.5f);
        canDash = true;
    }

    IEnumerator Stun(float time)
    {
        canMove = false;
        yield return new WaitForSeconds(time);
        canMove = true;
    }

    IEnumerator MakeInvincible(float time)
    {
        invincible = true;
        yield return new WaitForSeconds(time);
        invincible = false;
    }

    IEnumerator WaitToDead()
    {
        animator.SetBool("IsDead", true);
        canMove = false;
        invincible = true;
        yield return new WaitForSeconds(0.4f);
    }

    IEnumerator WaitToCheck(float time)
    {
        canCheck = false;
        yield return new WaitForSeconds(time);
        canCheck = true;
    }

    IEnumerator WaitToEndSliding()
    {
        yield return new WaitForSeconds(0.1f);
        canDoubleJump = true;
        isWallSliding = false;
        animator.SetBool("IsWallSliding", false);
        oldWallSlidding = false;
        m_WallCheck.localPosition = new Vector3(Mathf.Abs(m_WallCheck.localPosition.x), m_WallCheck.localPosition.y, 0);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (healthBar == null)
        {
            GameObject found = GameObject.FindWithTag("HealthBar");
            if (found != null)
                healthBar = found.GetComponent<Image>();
        }
    }

    public void SyncFacingDirection()
    {
        m_FacingRight = transform.localScale.x > 0;
    }
}
