using System.Collections;
using UnityEngine;
using Cinemachine;

public class SwitchCam : MonoBehaviour
{
    public CinemachineVirtualCamera camNormal;       // Camera theo dõi player
    public CinemachineVirtualCamera camWarningWide;  // Camera zoom trap góc rộng
    public CinemachineVirtualCamera camWarningClose; // Camera zoom trap góc gần

    public float wideDuration = 1.5f;  // Thời gian góc rộng
    public float closeDuration = 1.5f; // Thời gian góc gần

    private bool hasTriggered = false;

    private void Start()
    {
        // ✅ Đặt camera mặc định là camera player
        CameraManager.SwitchCamera(camNormal);
    }

    // Gọi từ trigger để bắt đầu quá trình chuyển camera
    public void TriggerWarningCamera()
    {
        if (!hasTriggered)
        {
            hasTriggered = true;
            StartCoroutine(SwitchCameraSequence());
        }
    }

    private IEnumerator SwitchCameraSequence()
    {
        // Khóa di chuyển player nếu cần
        LockPlayerMovement(false);

        // Lần 1: Camera góc rộng
        CameraManager.SwitchCamera(camWarningWide);
        yield return new WaitForSeconds(wideDuration);

        // Lần 2: Camera góc gần
        CameraManager.SwitchCamera(camWarningClose);
        yield return new WaitForSeconds(closeDuration);

        // Lần 3: Quay lại camera theo dõi player
        CameraManager.SwitchCamera(camNormal);

        // Mở di chuyển lại
        LockPlayerMovement(true);
    }

    void LockPlayerMovement(bool state)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerMovement movement = player.GetComponent<PlayerMovement>();
            if (movement != null)
            {
                movement.SetCanMove(state);
            }
        }
    }
}
