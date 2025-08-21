using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResumeBootstrap : MonoBehaviour
{
    public static ResumeBootstrap Instance { get; private set; }

    [Header("Assign in Inspector")]
    public GameObject playerPrefab;   // Prefab Player (tag "Player")
    public GameObject cameraPrefab;   // (Optional) Prefab Camera có sẵn CameraFollow; nếu null sẽ tự tạo Runtime

    private bool resumePending;
    private string resumeScene;
    private Vector3 resumePos;
    private float resumeHealth, resumeMaxHealth;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // gọi từ LoginManager khi có save
    public void StartResume(string sceneName, Vector3 pos, float health, float maxHealth)
    {
        resumePending = true;
        resumeScene = sceneName;
        resumePos = pos;
        resumeHealth = health;
        resumeMaxHealth = maxHealth;

        // Load thẳng scene đã lưu
        SceneManager.LoadSceneAsync(resumeScene);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!resumePending) return;
        if (scene.name != resumeScene) return;

        // 1) Tìm player trong scene
        var player = GameObject.FindGameObjectWithTag("Player");

        // 2) Nếu không có → spawn prefab
        if (player == null && playerPrefab != null)
        {
            player = Instantiate(playerPrefab); // thuộc scene, KHÔNG DDOL
        }

        if (player != null)
        {
            // Lấy các thành phần cần thiết
            var rb = player.GetComponent<Rigidbody2D>();
            var ctrl = player.GetComponent<CharacterController2D>();

            // 2.1) Tắt physics tạm thời để teleport an toàn
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.simulated = false;
            }

            // 2.2) Đặt vị trí
            player.transform.position = resumePos;

            // 2.3) Áp máu nếu có controller
            if (ctrl != null)
            {
                ctrl.maxLife = resumeMaxHealth;
                ctrl.life = Mathf.Clamp(resumeHealth, 0, ctrl.maxLife);
                ctrl.SyncFacingDirection();
            }

            // 2.4) Bật lại physics sau 1 frame (chạy coroutine trên Player nếu có)
            if (rb != null)
            {
                if (ctrl != null) ctrl.StartCoroutine(EnablePhysicsNextFrame(rb));
                else StartCoroutine(EnablePhysicsNextFrame(rb));
            }

            // 2.5) Đảm bảo có Camera và follow Player
            EnsureCameraAndFollow(player.transform);
        }
        else
        {
            Debug.LogError("[ResumeBootstrap] Không tìm thấy PlayerPrefab hoặc không spawn được Player!");
        }

        // Hoàn tất resume
        resumePending = false;

        // ❗ Đừng Destroy ngay để tránh hủy coroutine
        StartCoroutine(DestroyAfterFrames(2));
    }

    private void EnsureCameraAndFollow(Transform player)
    {
        // Tìm camera hiện có
        var mainCam = Camera.main;
        if (mainCam == null)
        {
            // Không có MainCamera → spawn
            if (cameraPrefab != null)
            {
                var camGO = Instantiate(cameraPrefab);
                // đảm bảo tag đúng
                if (camGO.GetComponent<Camera>() && camGO.tag != "MainCamera")
                    camGO.tag = "MainCamera";
                mainCam = camGO.GetComponent<Camera>();
                if (mainCam == null) mainCam = camGO.AddComponent<Camera>();
            }
            else
            {
                // Tạo Runtime camera tối thiểu
                var camGO = new GameObject("Main Camera");
                camGO.tag = "MainCamera";
                mainCam = camGO.AddComponent<Camera>();
                camGO.AddComponent<AudioListener>(); // 2D thường cần 1 listener
                // Thêm CameraFollow nếu thiếu
                if (camGO.GetComponent<CameraFollow>() == null)
                    camGO.AddComponent<CameraFollow>();
            }
        }

        // Đảm bảo có CameraFollow
        var follow = mainCam.GetComponent<CameraFollow>();
        if (follow == null) follow = mainCam.gameObject.AddComponent<CameraFollow>();

        // Set target rõ ràng
        follow.SetTarget(player);

        // Đảm bảo chỉ có 1 AudioListener (Unity chỉ cho phép 1)
        var listeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        if (listeners.Length > 1)
        {
            // giữ cái trên MainCamera, disable các cái còn lại
            foreach (var al in listeners)
            {
                if (al.gameObject != mainCam.gameObject)
                    al.enabled = false;
            }
            if (mainCam.GetComponent<AudioListener>() == null)
                mainCam.gameObject.AddComponent<AudioListener>();
        }
        else if (listeners.Length == 0)
        {
            mainCam.gameObject.AddComponent<AudioListener>();
        }
    }

    private IEnumerator EnablePhysicsNextFrame(Rigidbody2D rb)
    {
        yield return null; // đợi 1 frame
        if (rb != null) rb.simulated = true;
    }

    private IEnumerator DestroyAfterFrames(int frames)
    {
        for (int i = 0; i < frames; i++) yield return null;
        Destroy(gameObject);
    }
}
