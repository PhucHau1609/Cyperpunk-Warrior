using UnityEngine;

public class WallShrinker : MonoBehaviour
{
    public Transform leftWall;
    public Transform rightWall;
    public float shrinkSpeed = 2f;

    [Header("Target X Positions")]
    public float leftTargetX;
    public float rightTargetX;

    private bool isShrinking = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isShrinking = true;
        }
    }

    void Update()
    {
        if (isShrinking)
        {
            // Di chuyển LeftWall đến leftTargetX
            Vector2 leftTargetPos = new Vector2(leftTargetX, leftWall.position.y);
            leftWall.position = Vector2.MoveTowards(leftWall.position, leftTargetPos, shrinkSpeed * Time.deltaTime);

            // Di chuyển RightWall đến rightTargetX
            Vector2 rightTargetPos = new Vector2(rightTargetX, rightWall.position.y);
            rightWall.position = Vector2.MoveTowards(rightWall.position, rightTargetPos, shrinkSpeed * Time.deltaTime);

            // Dừng lại nếu cả 2 đã đến gần vị trí đích
            if (Mathf.Approximately(leftWall.position.x, leftTargetX) &&
                Mathf.Approximately(rightWall.position.x, rightTargetX))
            {
                isShrinking = false;
            }
        }
    }
}
