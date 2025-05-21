using UnityEngine;

public class CameraTriggerZone : MonoBehaviour
{
    public SwitchCam switchCam;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            switchCam.TriggerWarningCamera();
            gameObject.SetActive(false); // Chỉ kích hoạt 1 lần
        }
    }
}
