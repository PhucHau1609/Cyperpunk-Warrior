using UnityEngine;

public class Enemy_Bomb : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public Transform bombPoint;
    public GameObject bombPrefab;
    public float moveSpeed = 2f;
    public float dropRangeX = 0.5f; // Khoảng cách X để thả bom

    private Vector3 nextPoint;
    private Animator anim;
    private Transform player;

    void Start()
    {
        anim = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        nextPoint = pointB.position;
    }

    void Update()
    {
        Patrol();

        if (player != null && Mathf.Abs(player.position.x - transform.position.x) < dropRangeX)
        {
            anim.SetTrigger("DropBomb");
        }
    }

    void Patrol()
    {
        transform.position = Vector2.MoveTowards(transform.position, nextPoint, moveSpeed * Time.deltaTime);

        Vector2 dir = nextPoint - transform.position;
        if (dir.x > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else
            transform.localScale = new Vector3(-1, 1, 1);

        if (Vector2.Distance(transform.position, nextPoint) < 0.1f)
        {
            nextPoint = nextPoint == pointA.position ? pointB.position : pointA.position;
        }
    }

    // Gọi từ animation Event
    public void DropBomb()
    {
        Instantiate(bombPrefab, bombPoint.position, Quaternion.identity);
    }

    // Vẽ vùng phát hiện theo trục X
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(
            new Vector3(transform.position.x - dropRangeX, transform.position.y - 1, 0),
            new Vector3(transform.position.x - dropRangeX, transform.position.y + 1, 0)
        );

        Gizmos.DrawLine(
            new Vector3(transform.position.x + dropRangeX, transform.position.y - 1, 0),
            new Vector3(transform.position.x + dropRangeX, transform.position.y + 1, 0)
        );
    }
}
