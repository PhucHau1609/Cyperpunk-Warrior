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
    public float cameraMoveSpeed = 2f;
    public float focusDuration = 2f;         // thời gian camera dừng ở cổng

    [Header("Âm thanh")]
    public AudioClip switchSound;            // âm thanh khi bật công tắc
    public AudioClip doorOpenSound;          // âm thanh cửa mở
    private AudioSource audioSource;

    private Animator animator;
    private bool isPlayerInRange = false;
    private bool isSwitchOn = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        isSwitchOn = false;

        if (animator != null)
            animator.SetBool("isOn", false);
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

        // Ngắt follow player
        CameraFollow.Instance.IsPreviewing = true;

        // Camera move đến cổng
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * cameraMoveSpeed;
            Vector3 targetPos = cameraFocusPoint.position;
            targetPos.z = -10;
            CameraFollow.Instance.transform.position = Vector3.Lerp(CameraFollow.Instance.transform.position, targetPos, t);
            yield return null;
        }

        // Âm thanh mở cửa
        if (doorOpenSound != null && audioSource != null)
            audioSource.PlayOneShot(doorOpenSound);

        // Mở 2 cánh cửa
        Vector3 leftTarget = leftDoor.position + Vector3.left * doorOpenDistance;
        Vector3 rightTarget = rightDoor.position + Vector3.right * doorOpenDistance;

        t = 0f;
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
        yield return new WaitForSeconds(focusDuration);

        // Trả lại follow player
        CameraFollow.Instance.IsPreviewing = false;
        CameraFollow.Instance.TryFindPlayer();
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
