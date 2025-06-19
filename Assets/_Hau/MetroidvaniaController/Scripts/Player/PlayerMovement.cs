using UnityEngine;
using UnityEngine.Rendering.Universal; // THÊM NÀY nếu bạn dùng Light 2D URP
using System;

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

    [Header("Movement Control")]
    public bool canMove = true; // ⚠️ MỚI: Cho phép di chuyển


    void Awake()
    {
        DontDestroyOnLoad(this.gameObject); 
    }

    void Start()
    {
        GetComponentInChildren<Animator>().SetTrigger("PlayAppear");
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
            dash = true;

       /* // Bật/tắt tàng hình bằng phím J
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
        }*/
    }

    void FixedUpdate()
    {
        if (!canMove)
        {
            controller.Move(0f, false, false); // Ngừng di chuyển
            return;
        }

        controller.Move(horizontalMove * Time.fixedDeltaTime, jump, dash);
        jump = false;
        dash = false;
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

/*    void OnDestroy()
    {
        Debug.LogError($"[OnDestroy] Player bị xóa. Tên: {gameObject.name}, Time: {Time.time}, Scene: {gameObject.scene.name}");
        Debug.LogError(Environment.StackTrace);
    }*/


}
