using UnityEngine;

public class SceneBarrier : MonoBehaviour
{
    [Header("Barrier Settings")]
    [SerializeField] private GameObject barrierObject;
    [SerializeField] private bool startEnabled = true;
    [SerializeField] private bool autoRegister = true;

    [Header("Visual Feedback")]
    [SerializeField] private string barrierMessage = "Tiêu diệt tất cả enemy để tiếp tục!";
    [SerializeField] private Color barrierColor = Color.red;

    [Header("Effects")]
    [SerializeField] private ParticleSystem disableEffect;
    [SerializeField] private AudioClip disableSound;

    private bool isEnabled = true;
    private Collider2D barrierCollider;
    private SpriteRenderer barrierRenderer;
    private AudioSource audioSource;

    void Awake()
    {
        // Auto-setup nếu chưa có barrierObject
        if (barrierObject == null)
            barrierObject = gameObject;

        barrierCollider = barrierObject.GetComponent<Collider2D>();
        barrierRenderer = barrierObject.GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        // Tạo AudioSource nếu chưa có
        if (audioSource == null && disableSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    void Start()
    {
        // Tự động đăng ký với EnemyManager
        if (autoRegister && EnemyManager.Instance != null)
        {
            EnemyManager.Instance.RegisterBarrier(this);
        }

        // Set trạng thái ban đầu
        if (startEnabled)
            EnableBarrier();
        else
            DisableBarrier();
    }

    void OnDestroy()
    {
        // Hủy đăng ký khi bị destroy
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.UnregisterBarrier(this);
        }
    }

    /// <summary>
    /// Bật barrier
    /// </summary>
    public void EnableBarrier()
    {
        if (!isEnabled)
        {
            isEnabled = true;

            if (barrierObject != null)
                barrierObject.SetActive(true);

            if (barrierCollider != null)
                barrierCollider.enabled = true;

            if (barrierRenderer != null)
            {
                var color = barrierRenderer.color;
                color.a = 1f;
                barrierRenderer.color = color;
            }
        }
    }

    /// <summary>
    /// Tắt barrier
    /// </summary>
    public void DisableBarrier()
    {
        if (isEnabled)
        {
            isEnabled = false;

            // Chạy effect trước khi tắt
            PlayDisableEffects();

            if (barrierObject != null)
                barrierObject.SetActive(false);

            if (barrierCollider != null)
                barrierCollider.enabled = false;

            if (barrierRenderer != null)
            {
                var color = barrierRenderer.color;
                color.a = 0f;
                barrierRenderer.color = color;
            }
        }
    }

    /// <summary>
    /// Chạy các effect khi tắt barrier
    /// </summary>
    private void PlayDisableEffects()
    {
        // Particle effect
        if (disableEffect != null)
        {
            disableEffect.Play();
        }

        // Sound effect
        if (audioSource != null && disableSound != null)
        {
            audioSource.PlayOneShot(disableSound);
        }
    }

    /// <summary>
    /// Khi player va chạm với barrier
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isEnabled && other.CompareTag("Player"))
        {
            ShowBlockMessage();
        }
    }

    /// <summary>
    /// Hiển thị thông báo khi bị chặn
    /// </summary>
    private void ShowBlockMessage()
    {
        if (EnemyManager.Instance != null)
        {
            int remaining = EnemyManager.Instance.GetRemainingKills();
            string message = $"{barrierMessage}\nCòn lại: {remaining} enemy";

            // Hiển thị message (có thể dùng UI Manager hoặc Debug)
            Debug.Log($"[SceneBarrier] {message}");

            // TODO: Hiển thị UI notification
            // UIManager.Instance?.ShowNotification(message);
        }
    }

    /// <summary>
    /// Vẽ gizmo trong Scene view
    /// </summary>
    void OnDrawGizmos()
    {
        if (barrierObject == null) return;

        Gizmos.color = isEnabled ? barrierColor : Color.green;

        var collider = barrierObject.GetComponent<Collider2D>();
        if (collider != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;

            if (collider is BoxCollider2D box)
            {
                Gizmos.DrawWireCube(box.offset, box.size);
            }
            else if (collider is CircleCollider2D circle)
            {
                Gizmos.DrawWireSphere(circle.offset, circle.radius);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (barrierObject == null) return;

        Gizmos.color = barrierColor;
        Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.3f);

        var collider = barrierObject.GetComponent<Collider2D>();
        if (collider != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;

            if (collider is BoxCollider2D box)
            {
                Gizmos.DrawCube(box.offset, box.size);
            }
            else if (collider is CircleCollider2D circle)
            {
                Gizmos.DrawSphere(circle.offset, circle.radius);
            }
        }
    }

    // Getters
    public bool IsEnabled => isEnabled;
    public string GetBarrierMessage() => barrierMessage;

    // Setters (cho Editor Tool)
    public void SetBarrierObject(GameObject obj) => barrierObject = obj;
    public void SetBarrierMessage(string message) => barrierMessage = message;
    public void SetStartEnabled(bool enabled) => startEnabled = enabled;
}