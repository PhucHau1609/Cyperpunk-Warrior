using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraSwitch : MonoBehaviour
{
    public Transform trapFocusPoint;
    public float previewDuration = 3f;
    public float moveSpeed = 2f;

    public bool isPreviewing = false;
    private Transform camTransform;

    void Start()
    {
        if (CameraFollow.Instance != null)
        {
            camTransform = CameraFollow.Instance.transform;
        }
        else
        {
            Debug.LogWarning("CameraFollow instance not found!");
            return;
        }

        // Xoá tất cả camera trùng nếu có
        RemoveDuplicateMainCameras();

        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "map1level4" || sceneName == "map2level2") 
        {
            StartCoroutine(PreviewTrap());
        }
    }

    IEnumerator PreviewTrap()
    {
        isPreviewing = true;

        CameraFollow.Instance.enabled = false;

        Vector3 startPos = camTransform.position;
        Vector3 targetPos = new Vector3(trapFocusPoint.position.x, trapFocusPoint.position.y, camTransform.position.z);

        while (Vector3.Distance(camTransform.position, targetPos) > 0.1f)
        {
            camTransform.position = Vector3.Lerp(camTransform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(previewDuration);

        Vector3 playerPos = new Vector3(CameraFollow.Instance.Target.position.x, CameraFollow.Instance.Target.position.y, camTransform.position.z);
        while (Vector3.Distance(camTransform.position, playerPos) > 0.1f)
        {
            camTransform.position = Vector3.Lerp(camTransform.position, playerPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        CameraFollow.Instance.enabled = true;
        isPreviewing = false;
    }

    void RemoveDuplicateMainCameras()
    {
        GameObject[] mainCams = GameObject.FindGameObjectsWithTag("MainCamera");

        foreach (GameObject camObj in mainCams)
        {
            if (CameraFollow.Instance == null) continue;

            if (camObj != CameraFollow.Instance.gameObject)
            {
                Destroy(camObj);
            }
        }
    }
}