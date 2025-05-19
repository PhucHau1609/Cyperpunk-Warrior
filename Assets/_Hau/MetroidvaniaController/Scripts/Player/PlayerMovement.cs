using UnityEngine;
using UnityEngine.Rendering.Universal; // THÊM NÀY nếu bạn dùng Light 2D URP

public class PlayerMovement : MonoBehaviour
{
    public CharacterController2D controller;
    public Animator animator;
    public float runSpeed = 40f;

    private float horizontalMove = 0f;
    private bool jump = false;
    private bool dash = false;

    private SpriteRenderer spriteRenderer;
    private bool isInvisible = false;

    [Header("Invisibility Light")]
    public Light2D invisibilityLight; // GÁN OBJECT NÀY TRONG INSPECTOR

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    void Start()
    {
        GetComponent<Animator>().SetTrigger("PlayAppear");
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
            return;
        // Di chuyển
        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;
        animator.SetFloat("Speed", Mathf.Abs(horizontalMove));

        if (Input.GetKeyDown(KeyCode.Space))
            jump = true;

        if (Input.GetKeyDown(KeyCode.E))
            dash = true;

        // Bật/tắt tàng hình bằng phím J
        if (Input.GetKeyDown(KeyCode.J))
        {
            isInvisible = !isInvisible;

            // Bật/tắt Spot Light
            if (invisibilityLight != null)
            {
                invisibilityLight.enabled = !isInvisible;
            }

            Debug.Log("Tàng hình: " + isInvisible);
        }
    }

    void FixedUpdate()
    {
        controller.Move(horizontalMove * Time.fixedDeltaTime, jump, dash);
        jump = false;
        dash = false;
    }

    public void OnFall()
    {
        animator.SetBool("IsJumping", true);
    }

    public void OnLanding()
    {
        animator.SetBool("IsJumping", false);
    }

    public bool IsInvisible()
    {
        return isInvisible;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("MovingTrap"))
        {
            transform.parent = collision.transform;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("MovingTrap"))
        {
            transform.parent = null;
        }
    }
}
