using UnityEngine;

public class FloatingFollower : MonoBehaviour
{
    public Transform player;
    public float followHeight = 1.5f;
    public float sideOffset = 1f;
    public float xFollowSpeed = 2f;
    public float yFollowSpeed = 5f;

    public bool isDashing = false;
    public float dashFollowMultiplier = 2f;

    private Vector3 targetPos;
    private Rigidbody2D rb;
    private Animator anim;

    private enum PetState { Sleepwell, Awaken, Following }
    private PetState state = PetState.Sleepwell;

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        if (player == null) return;

        if (state == PetState.Sleepwell)
        {
            if (CodeLock.PetUnlocked)
            {
                state = PetState.Awaken;
                anim.SetTrigger("Awaken");
                StartCoroutine(StartFollowAfterDelay(1.5f)); // thời gian animation awaken
            }
            return;
        }

        if (state != PetState.Following) return;

        // Pet follow logic
        float direction = player.localScale.x > 0 ? -1 : 1;
        targetPos = player.position + new Vector3(sideOffset * direction, followHeight, 0);

        float heightDiff = player.position.y - rb.position.y;
        float ySpeed = yFollowSpeed;

        if (heightDiff < -1f) ySpeed *= 4f;
        else if (heightDiff > 1f) ySpeed *= 1.5f;

        float speedMultiplier = isDashing ? dashFollowMultiplier : 1f;

        float newX = Mathf.Lerp(rb.position.x, targetPos.x, xFollowSpeed * speedMultiplier * Time.fixedDeltaTime);
        float newY = Mathf.Lerp(rb.position.y, targetPos.y, ySpeed * speedMultiplier * Time.fixedDeltaTime);

        rb.MovePosition(new Vector2(newX, newY));

        transform.localScale = direction > 0 ? new Vector3(-2, 2, 2) : new Vector3(2, 2, 2);
    }

    System.Collections.IEnumerator StartFollowAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        state = PetState.Following;
        // Lúc này animation sẽ tự động sang Idle do đã setup trong Animator
    }
}




//using UnityEngine;

//public class FloatingFollower : MonoBehaviour
//{
//    public Transform player;       // Gán Player vào đây
//    public Vector3 baseOffset = new Vector3(1.5f, 2f, 0);
//    public float followSpeed = 5f; // Tốc độ bay theo
//    private Vector3 currentOffset;
//    private Vector3 lastPlayerPos;

//    void Start()
//    {
//        currentOffset = baseOffset;
//        lastPlayerPos = player.position;
//    }

//    void Update()
//    {
//        // Xác định hướng di chuyển của player (trái hay phải)
//        float movement = player.position.x - lastPlayerPos.x;

//        if (Mathf.Abs(movement) > 0.01f)
//        {
//            currentOffset.x = movement > 0 ? -Mathf.Abs(baseOffset.x) : Mathf.Abs(baseOffset.x);
//        }

//        lastPlayerPos = player.position;

//        // Theo dõi vị trí bay theo
//        Vector3 targetPosition = player.position + currentOffset;
//        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
//    }
//}
