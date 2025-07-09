using UnityEngine;

public class FlyingObject : MonoBehaviour
{
    public float speed = 5f;
    public float amplitude = 0.5f;     // Biên độ dao động sóng sin (lên xuống)
    public float frequency = 1f;       // Tần số sóng sin

    public bool moveLeftToRight = true;
    public float respawnX = -20f;      // Vị trí X để respawn lại
    public float endX = 20f;           // Khi tới đây thì quay lại đầu

    private float initialY;

    void Start()
    {
        initialY = transform.position.y;
    }

    void Update()
    {
        float moveDir = moveLeftToRight ? 1f : -1f;
        float moveX = speed * moveDir * Time.deltaTime;
        float offsetY = Mathf.Sin(Time.time * frequency) * amplitude;

        transform.Translate(new Vector3(moveX, offsetY * Time.deltaTime, 0f));

        // Nếu bay khỏi màn hình thì reset về đầu
        if (moveLeftToRight && transform.position.x > endX)
        {
            Respawn();
        }
        else if (!moveLeftToRight && transform.position.x < respawnX)
        {
            Respawn();
        }
    }

    void Respawn()
    {
        float startX = moveLeftToRight ? respawnX : endX;
        float randomY = initialY + Random.Range(-1f, 1f); // Ngẫu nhiên cao thấp
        transform.position = new Vector3(startX, randomY, transform.position.z);
    }
}
