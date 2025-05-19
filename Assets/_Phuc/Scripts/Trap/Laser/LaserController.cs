using UnityEngine;
using System.Collections;

public class LaserController : MonoBehaviour
{
    [SerializeField] private float minX = -0.53f;
    [SerializeField] private float maxX = 10.17f;
    [SerializeField] private float speed = 2f;
    [SerializeField] private GameObject laserObject; // Gán laser (vd: tia đỏ) trong Inspector
    [SerializeField] private float startDelay = 0f;  // Thêm delay để đồng bộ nhiều laser

    private bool movingRight = true;
    private bool isWaiting = true; // mặc định chờ nếu có delay

    void Start()
    {
        StartCoroutine(StartWithDelay());
    }

    IEnumerator StartWithDelay()
    {
        yield return new WaitForSeconds(startDelay);
        isWaiting = false;
    }

    void Update()
    {
        if (!isWaiting)
        {
            float moveStep = speed * Time.deltaTime;

            if (movingRight)
            {
                transform.position += new Vector3(moveStep, 0, 0);
                if (transform.position.x >= maxX)
                {
                    transform.position = new Vector3(maxX, transform.position.y, transform.position.z);
                    StartCoroutine(WaitAndSwitchDirection());
                }
            }
            else
            {
                transform.position -= new Vector3(moveStep, 0, 0);
                if (transform.position.x <= minX)
                {
                    transform.position = new Vector3(minX, transform.position.y, transform.position.z);
                    StartCoroutine(WaitAndSwitchDirection());
                }
            }

            if (laserObject != null && !laserObject.activeSelf)
                laserObject.SetActive(true);
        }
        else
        {
            if (laserObject != null && laserObject.activeSelf)
                laserObject.SetActive(false);
        }
    }

    IEnumerator WaitAndSwitchDirection()
    {
        isWaiting = true;
        if (laserObject != null)
            laserObject.SetActive(false);

        yield return new WaitForSeconds(0.5f); // thời gian dừng lại
        movingRight = !movingRight;
        isWaiting = false;
    }
}
