using UnityEngine;

public class RotateSpinner : MonoBehaviour
{
    public float speed = 180f;
    private Quaternion targetRotation;

    void Start()
    {
        targetRotation = transform.rotation;
    }

    void Update()
    {
        // Tăng góc quay đều theo thời gian
        targetRotation *= Quaternion.Euler(0, 0, speed * Time.deltaTime);
        // Quay mượt đến góc mới
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.1f);
    }
}
