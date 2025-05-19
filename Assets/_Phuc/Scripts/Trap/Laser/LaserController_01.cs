using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserController_01 : MonoBehaviour
{
    [SerializeField] private GameObject targetObject; // Gán object cần bật/tắt
    [SerializeField] private float toggleInterval = 1f; // Thời gian giữa các lần bật/tắt

    private void Start()
    {
        StartCoroutine(ToggleRoutine());
    }

    private IEnumerator ToggleRoutine()
    {
        while (true)
        {
            if (targetObject != null)
                targetObject.SetActive(!targetObject.activeSelf);

            yield return new WaitForSeconds(toggleInterval);
        }
    }
}
