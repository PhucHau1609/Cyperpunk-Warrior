using UnityEngine;

public class AutoCameraMover : MonoBehaviour
{
    public float speed = 2f; // Tốc độ di chuyển theo trục X

    void Update()
    {
        transform.position += Vector3.right * speed * Time.deltaTime;
    }
}
