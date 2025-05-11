using UnityEngine;

public class MovingTrap_01 : MonoBehaviour
{
    [SerializeField] private float minY = -0.53f;
    [SerializeField] private float maxY = 10.17f;
    [SerializeField] private float speed = 2f;

    private bool movingUp = true;

    void Update()
    {
        float moveStep = speed * Time.deltaTime;

        if (movingUp)
        {
            transform.position += new Vector3(0, moveStep, 0);
            if (transform.position.y >= maxY)
            {
                transform.position = new Vector3(transform.position.x, maxY, transform.position.z);
                movingUp = false;
            }
        }
        else
        {
            transform.position -= new Vector3(0, moveStep, 0);
            if (transform.position.y <= minY)
            {
                transform.position = new Vector3(transform.position.x, minY, transform.position.z);
                movingUp = true;
            }
        }
    }
}
