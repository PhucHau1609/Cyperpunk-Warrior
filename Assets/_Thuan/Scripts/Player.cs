using UnityEngine;
using UnityEngine.Rendering.Universal; // THÊM NÀY nếu bạn dùng Light 2D URP

public class Player : MonoBehaviour
{
    public CharacterController2D controller;
    public Animator animator;
    public float runSpeed = 40f;

    private float horizontalMove = 0f;
    private bool jump = false;
    private bool dashX = false; // ⚠️ THAY ĐỔI: dash ngang
    private bool dashY = false; // ⚠️ MỚI: dash dọc

    private SpriteRenderer spriteRenderer;
   private bool isInvisible = false;

     [Header("Invisibility Light")]
     public Light2D invisibilityLight; // GÁN OBJECT NÀY TRONG INSPECTOR

    [Header("Movement Control")]
    public bool canMove = true; // ⚠️ MỚI: Cho phép di chuyển

    // private void Awake()
    // {
    //     DontDestroyOnLoad(this);
    // }

    void Start()
    {
        GetComponent<Animator>().SetTrigger("PlayAppear");
        //spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (!canMove) return; // ⚠️ MỚI: Nếu bị khóa thì không làm gì

        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
            return;

        // Di chuyển
        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;
        animator.SetFloat("Speed", Mathf.Abs(horizontalMove));

        if (Input.GetKeyDown(KeyCode.Space))
            jump = true;

        if (Input.GetKeyDown(KeyCode.E))
            dashX = true; // ⚠️ THAY ĐỔI: dash ngang

        if (Input.GetKeyDown(KeyCode.S))
            dashY = true; // ⚠️ MỚI: dash dọc

        // Bật/tắt tàng hình bằng phím J
        if (Input.GetKeyDown(KeyCode.J))
        {
            isInvisible = !isInvisible;

           // Bật/tắt Spot Light
           if (invisibilityLight != null)
           {
                invisibilityLight.enabled = !isInvisible;
            }

        //     // Thay đổi độ alpha của nhân vật
            if (spriteRenderer != null)
            {
               Color color = spriteRenderer.color;
               color.a = isInvisible ? 0.1490196f : 1f;
               spriteRenderer.color = color;
          }

            Debug.Log("Tàng hình: " + isInvisible);
        }
    }

    void FixedUpdate()
    {
        if (!canMove)
        {
            controller.Move(0f, false, false, false); // ⚠️ THAY ĐỔI: thêm tham số dashY
            return;
        }

        controller.Move(horizontalMove * Time.fixedDeltaTime, jump, dashX, dashY); // ⚠️ THAY ĐỔI: thêm dashY
        jump = false;
        dashX = false; // ⚠️ THAY ĐỔI
        dashY = false; // ⚠️ MỚI
    }

    public void SetCanMove(bool state) // ⚠️ MỚI: Hàm khóa/mở di chuyển
    {
        canMove = state;
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
