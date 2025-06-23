using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserManagerTrap  : MonoBehaviour
{
    public List<GameObject> lasers;       // Danh sách laser đã tắt sẵn
    public float delayBetweenLasers = 0.2f; // Khoảng cách thời gian giữa từng laser

    public void ActivateLasers()
    {
        StartCoroutine(ActivateLasersInOrder());
    }

    private IEnumerator ActivateLasersInOrder()
    {
        foreach (GameObject laser in lasers)
        {
            laser.SetActive(true); // Mở laser lên
            yield return new WaitForSeconds(delayBetweenLasers);
        }
    }
}
