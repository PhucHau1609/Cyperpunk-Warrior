using UnityEngine;

public class MovingTrap : MonoBehaviour
{
    [SerializeField] private float minX = -0.53f;
    [SerializeField] private float maxX = 10.17f;
    [SerializeField] private float speed = 2f;

    private bool movingRight = true;


    void Update()
    {
        float moveStep = speed * Time.deltaTime;

        if (movingRight)
        {
            transform.position += new Vector3(moveStep, 0, 0);
            if (transform.position.x >= maxX)
            {
                transform.position = new Vector3(maxX, transform.position.y, transform.position.z);
                movingRight = false;
            }
        }
        else
        {
            transform.position -= new Vector3(moveStep, 0, 0);
            if (transform.position.x <= minX)
            {
                transform.position = new Vector3(minX, transform.position.y, transform.position.z);
                movingRight = true;
            }
        }
    }
}
