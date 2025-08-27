/*using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraBoundaries : MonoBehaviour
{
    [Header("Giới hạn vùng di chuyển của Camera")]
    public float minX;
    public float maxX;
    public float minY;
    public float maxY;

    private Camera cam;
    private float halfHeight;
    private float halfWidth;

    private void Awake()
    {
        cam = Camera.main;
        if (cam == null)
        {
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        UpdateCameraSize();
    }
    
    // Thêm method này vào cuối class
    void Update()
    {
        // Cập nhật camera size liên tục để theo dõi thay đổi zoom
        UpdateCameraSize();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        cam = Camera.main;
        UpdateCameraSize();
    }

    public void UpdateCameraSize()
    {
        if (cam == null) return;

        halfHeight = cam.orthographicSize;
        halfWidth = halfHeight * cam.aspect;
    }

    void LateUpdate()
    {
        if (CameraFollow.Instance == null || cam == null) return;

        Vector3 camPos = cam.transform.position;

        float clampedX = Mathf.Clamp(camPos.x, minX + halfWidth, maxX - halfWidth);
        float clampedY = Mathf.Clamp(camPos.y, minY + halfHeight, maxY - halfHeight);

        cam.transform.position = new Vector3(clampedX, clampedY, camPos.z);
    }
}
*/

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraBoundaries : MonoBehaviour
{
    [Header("Giới hạn vùng di chuyển của Camera")]
    public float minX;
    public float maxX;
    public float minY;
    public float maxY;

    private Camera cam;
    private float halfHeight;
    private float halfWidth;

    private Coroutine _findCamCo;

    private void OnEnable()
    {
        // Lắng nghe mỗi lần load scene để bắt lại camera
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Thử bắt camera ngay khi enable
        StartFindCameraLoop();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (_findCamCo != null)
        {
            StopCoroutine(_findCamCo);
            _findCamCo = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Mỗi scene mới: chạy lại vòng lặp tìm camera
        StartFindCameraLoop();
    }

    private void StartFindCameraLoop()
    {
        if (_findCamCo != null) StopCoroutine(_findCamCo);
        _findCamCo = StartCoroutine(FindCameraLoop());
    }

    private IEnumerator FindCameraLoop()
    {
        // Thử vài cách để lấy MainCamera, lặp đến khi có
        while (true)
        {
            // 1) Ưu tiên Camera.main
            cam = Camera.main;

            // 2) Nếu vẫn null, thử lấy từ CameraFollow.Instance
            if (cam == null && CameraFollow.Instance != null)
            {
                cam = CameraFollow.Instance.GetComponent<Camera>();
            }

            // 3) Nếu vẫn null, thử tìm camera bất kỳ trong scene
            if (cam == null)
            {
                var anyCam = FindFirstObjectByType<Camera>(FindObjectsInactive.Exclude);
                if (anyCam != null)
                {
                    cam = anyCam;
                    if (cam.CompareTag("MainCamera") == false)
                        cam.tag = "MainCamera"; // chuẩn hoá để lần sau Camera.main hoạt động
                }
            }

            if (cam != null)
            {
                UpdateCameraSize();
                _findCamCo = null;
                yield break;
            }

            // Chưa có camera → đợi 1 frame nữa (ResumeBootstrap sẽ spawn)
            yield return null;
        }
    }

    void Update()
    {
        // Nếu camera bị destroy/disable giữa chừng → thử bắt lại
        if (cam == null || !cam.isActiveAndEnabled)
        {
            StartFindCameraLoop();
            return;
        }

        // Theo dõi thay đổi zoom/aspect
        UpdateCameraSize();
    }

    public void UpdateCameraSize()
    {
        if (cam == null) return;

        halfHeight = cam.orthographicSize;
        halfWidth = halfHeight * cam.aspect;
    }

    void LateUpdate()
    {
        if (cam == null) return;

        // Clamp vị trí camera trong biên
        Vector3 camPos = cam.transform.position;

        float clampedX = Mathf.Clamp(camPos.x, minX + halfWidth, maxX - halfWidth);
        float clampedY = Mathf.Clamp(camPos.y, minY + halfHeight, maxY - halfHeight);

        cam.transform.position = new Vector3(clampedX, clampedY, camPos.z);
    }
}
