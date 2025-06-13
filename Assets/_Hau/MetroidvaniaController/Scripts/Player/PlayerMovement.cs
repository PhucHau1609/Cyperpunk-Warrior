using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController2D controller;
    public Animator animator;
    public float runSpeed = 40f;

    private float horizontalMove = 0f;
    private bool jump = false;
    private bool dash = false;

    [Header("Movement Control")]
    public bool canMove = true;

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        GetComponentInChildren<Animator>().SetTrigger("PlayAppear");
    }

    void Update()
    {
        if (!canMove) return;

        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
            return;

        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;
        animator.SetFloat("Speed", Mathf.Abs(horizontalMove));

        if (Input.GetKeyDown(KeyCode.Space))
            jump = true;

        if (Input.GetKeyDown(KeyCode.E))
            dash = true;
    }

    void FixedUpdate()
    {
        if (!canMove)
        {
            controller.Move(0f, false, false);
            return;
        }

        controller.Move(horizontalMove * Time.fixedDeltaTime, jump, dash);
        jump = false;
        dash = false;
    }

    public void SetCanMove(bool state)
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
