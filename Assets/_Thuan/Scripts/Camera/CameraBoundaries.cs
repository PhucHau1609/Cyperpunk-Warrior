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
