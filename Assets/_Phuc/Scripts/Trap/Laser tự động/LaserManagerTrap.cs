using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserManagerTrap : MonoBehaviour
{
    public List<GameObject> lasers;       // Laser từ 1 -> 9 (trái sang phải)
    public float delay = 0.2f;            // Thời gian giữa mỗi thao tác

    public void ActivateLasers()
    {
        StartCoroutine(LaserPatternSequence());
    }

    private IEnumerator LaserPatternSequence()
    {
        // BƯỚC 1: Mở laser 1 -> 5
        for (int i = 0; i <= 4; i++)
        {
            lasers[i].SetActive(true);
            yield return new WaitForSeconds(delay);
        }

        yield return new WaitForSeconds(0.5f);

        // BƯỚC 2: Mở-tắt theo cặp
        for (int i = 0; i < 4; i++)
        {
            int openIndex = 8 - i;   // 9,8,7,6
            int closeIndex = 4 - i;  // 5,4,3,2

            lasers[openIndex].SetActive(true);
            yield return new WaitForSeconds(delay / 2);

            lasers[closeIndex].SetActive(false);
            yield return new WaitForSeconds(delay);
        }

        // Tắt laser 1
        lasers[0].SetActive(false);
        yield return new WaitForSeconds(delay);

        // BƯỚC 2.5: Tắt laser 6 -> 9
        for (int i = 5; i <= 8; i++)
        {
            lasers[i].SetActive(false);
            yield return new WaitForSeconds(delay);
        }

        yield return new WaitForSeconds(0.5f);

        // BƯỚC 3: Mở lại toàn bộ laser 1 -> 9
        for (int i = 0; i < lasers.Count; i++)
        {
            lasers[i].SetActive(true);
            yield return new WaitForSeconds(delay);
        }
    }
    public void ResetTrap()
    {
        StopAllCoroutines(); // Dừng tất cả pattern đang chạy
        foreach (var laser in lasers)
        {
            if (laser != null)
                laser.SetActive(false);
        }
    }

}
