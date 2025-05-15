using UnityEngine;

public class NPCFollow : MonoBehaviour
{
    public Transform player;
    public float followHeight = 1.5f;
    public float sideOffset = 1f;
    public float xFollowSpeed = 2f;
    public float yFollowSpeed = 5f;

    public bool isDashing = false; // gắn từ PlayerController
    public float dashFollowMultiplier = 2f; // tăng tốc khi dash

    private Vector3 targetPos;
    private Rigidbody2D rb;

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (player == null) return;

        float direction = player.localScale.x > 0 ? -1 : 1;

        // Tính vị trí đích dựa trên vị trí player
        targetPos = player.position + new Vector3(sideOffset * direction, followHeight, 0);

        // Bay xuống nhanh hơn nếu player đang thấp
        float heightDiff = player.position.y - rb.position.y;

        // Nếu player đang ở thấp hơn nhiều → bay xuống nhanh hơn
        float ySpeed = yFollowSpeed;
        if (heightDiff < -1f) ySpeed *= 4f; // bay xuống nhanh
        else if (heightDiff > 1f) ySpeed *= 1.5f; // bay lên nhanh nhẹ

        // Dash theo
        float speedMultiplier = isDashing ? dashFollowMultiplier : 1f;

        // Di chuyển mượt theo trục X và Y với tốc độ khác nhau
        float newX = Mathf.Lerp(rb.position.x, targetPos.x, xFollowSpeed * Time.fixedDeltaTime);
        float newY = Mathf.Lerp(rb.position.y, targetPos.y, yFollowSpeed * Time.fixedDeltaTime);

        rb.MovePosition(new Vector2(newX, newY));

        // Xoay mặt NPC
        transform.localScale = direction > 0 ? new Vector3(-2, 2, 2) : new Vector3(2, 2, 2);
        
        //// Xoay mặt NPC
        //if (direction > 0)
        //    transform.localScale = new Vector3(-2, 2, 2); // nhìn phải
        //else
        //    transform.localScale = new Vector3(2, 2, 2); // nhìn trái
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
