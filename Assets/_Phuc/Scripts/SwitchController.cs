using UnityEngine;
using System.Collections;

public class SwitchController : MonoBehaviour
{
    [Header("Tham chiếu tới cửa")]
    public Transform leftDoor;
    public Transform rightDoor;
    public float doorOpenDistance = 2f;      // khoảng cách mỗi cánh mở ra
    public float doorOpenSpeed = 2f;         // tốc độ mở cửa

    [Header("Camera")]
    public Transform cameraFocusPoint;       // điểm focus camera vào cổng
    [Tooltip("Thời gian camera DI CHUYỂN tới điểm focus")]
    public float focusInTime = 1.2f;
    [Tooltip("Thời gian camera DI CHUYỂN trở về gần player")]
    public float focusOutTime = 1.2f;
    [Tooltip("Giữ camera ở cổng trong bao lâu")]
    public float focusHoldTime = 2f;
    [Tooltip("Đường cong easing cho vào/ra (x:0->1 là thời gian, y:0->1 là mức tiến)")]
    public AnimationCurve easeIn = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve easeOut = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Âm thanh")]
    public AudioClip switchSound;            // âm thanh khi bật công tắc
    public AudioClip doorOpenSound;          // âm thanh cửa mở
    private AudioSource audioSource;

    private Animator animator;
    private bool isPlayerInRange = false;
    private bool isSwitchOn = false;

    // cache
    private Transform camTf;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        isSwitchOn = false;

        if (animator != null)
            animator.SetBool("isOn", false);

        camTf = CameraFollow.Instance != null ? CameraFollow.Instance.transform : Camera.main.transform;
    }

    void Update()
    {
        if (isPlayerInRange && !isSwitchOn && Input.GetMouseButtonDown(0))
        {
            StartCoroutine(HandleSwitch());
        }
    }

    IEnumerator HandleSwitch()
    {
        isSwitchOn = true;

        // Animation công tắc
        if (animator != null)
            animator.SetBool("isOn", true);

        // Âm thanh công tắc
        if (switchSound != null && audioSource != null)
            audioSource.PlayOneShot(switchSound);

        // ⏳ Đợi 0.7s để thấy cảnh công tắc gạt xuống
        yield return new WaitForSeconds(0.7f);

        // Ngắt follow player và chuẩn bị di chuyển mượt
        if (CameraFollow.Instance != null)
            CameraFollow.Instance.IsPreviewing = true;

        Vector3 camStart = camTf.position;
        Vector3 camTarget = cameraFocusPoint != null ? cameraFocusPoint.position : camStart;
        camTarget.z = -10f;

        // Di chuyển mượt tới điểm focus
        yield return MoveCameraSmooth(camStart, camTarget, focusInTime, easeIn);

        yield return new WaitForSeconds(0.5f);

        // Âm thanh mở cửa
        if (doorOpenSound != null && audioSource != null)
            audioSource.PlayOneShot(doorOpenSound);

        // Mở 2 cánh cửa (mượt)
        Vector3 leftTarget = leftDoor.position + Vector3.left * doorOpenDistance;
        Vector3 rightTarget = rightDoor.position + Vector3.right * doorOpenDistance;

        float t = 0f;
        Vector3 leftStart = leftDoor.position;
        Vector3 rightStart = rightDoor.position;

        while (t < 1f)
        {
            t += Time.deltaTime * doorOpenSpeed;
            leftDoor.position = Vector3.Lerp(leftStart, leftTarget, t);
            rightDoor.position = Vector3.Lerp(rightStart, rightTarget, t);
            yield return null;
        }

        // Giữ camera ở cửa một lúc
        yield return new WaitForSeconds(focusHoldTime);

        // ===== Trả camera về gần player một cách MƯỢT =====
        // Lấy vị trí mục tiêu hiện tại của camera follow (xấp xỉ vị trí player)
        Vector3 backTarget = camTf.position; // fallback
        if (CameraFollow.Instance != null)
        {
            // Nếu CameraFollow có method lấy target thì dùng; ở đây mình ước lượng là vị trí player
            // hoặc gọi TryFindPlayer rồi lấy transform player nếu bạn có.
            // Tối thiểu: dời về vị trí hiện thời của player nếu có:
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                backTarget = playerObj.transform.position;
            else
                backTarget = camStart; // không tìm thấy thì quay lại chỗ cũ
        }
        backTarget.z = -10f;

        Vector3 camFocusEnd = camTf.position;
        yield return MoveCameraSmooth(camFocusEnd, backTarget, focusOutTime, easeOut);

        // Kết thúc preview và trả follow
        if (CameraFollow.Instance != null)
        {
            CameraFollow.Instance.IsPreviewing = false;
            CameraFollow.Instance.TryFindPlayer();
             QuestEventBus.Raise("Destroy_all");

        }
    }

    private IEnumerator MoveCameraSmooth(Vector3 from, Vector3 to, float duration, AnimationCurve curve)
    {
        if (duration <= 0f) duration = 0.0001f;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / duration);
            float eased = curve != null ? curve.Evaluate(u) : u;
            camTf.position = Vector3.LerpUnclamped(from, to, eased);
            yield return null;
        }
        camTf.position = to;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            isPlayerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            isPlayerInRange = false;
    }
}
