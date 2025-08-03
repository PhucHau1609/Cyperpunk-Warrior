using UnityEngine;
using UnityEngine.Rendering.Universal; // THÊM NÀY nếu bạn dùng Light 2D URP
using System;

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


    void Awake()
    {
        
    }

    void Start()
    {
        GetComponentInChildren<Animator>().SetTrigger("PlayAppear");
    }

    void Update()
    {
        if (!canMove || (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive))
        {
            horizontalMove = 0f;
            animator.SetFloat("Speed", 0f); // ⚠️ RESET animation chạy
            return;
        }

        // Di chuyển
        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;
        animator.SetFloat("Speed", Mathf.Abs(horizontalMove));

        if (Input.GetKeyDown(KeyCode.Space))
            jump = true;

     /*   if (Input.GetKeyDown(KeyCode.E) && PlayerStatus.Instance != null && controller.canDash)
        {
            PlayerStatus.Instance.UseEnergy(10f);
            PlayerStatus.Instance.TriggerBlink(PlayerStatus.Instance.eImage);
            dashX = true; // ⚠️ THAY ĐỔI: dash ngang

        }

        if (Input.GetKeyDown(KeyCode.S))
            dashY = true; // ⚠️ MỚI: dash dọc*/
    }

    public bool TriggerDashX()
    {
        if (controller.canDash && PlayerStatus.Instance != null && PlayerStatus.Instance.UseEnergy(10f))
        {
            PlayerStatus.Instance.TriggerBlink(PlayerStatus.Instance.eImage);
            dashX = true;
            return true;
        }
        return false;
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


