using UnityEngine;
using System.Collections;

public class SwitchController : MonoBehaviour
{
    [Header("Tham chiếu tới Laser và âm thanh")]
    public GameObject laserObject;         // Laser 1
    public GameObject laserObject_02;      // Laser 2
    public GameObject laserObject_03;      // Laser 3
    public AudioClip switchSound;          // Âm thanh khi bật công tắc

    private AudioSource audioSource;       // Phát âm thanh
    private Animator animator;             // Điều khiển animation công tắc

    private bool isPlayerInRange = false;  // Người chơi đang trong vùng công tắc
    private bool isSwitchOn = false;       // Trạng thái công tắc

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        isSwitchOn = false;

        if (animator != null)
            animator.SetBool("isOn", false);  // Animation ban đầu là Off
    }

    void Update()
    {
        if (isPlayerInRange && !isSwitchOn && Input.GetMouseButtonDown(0))
        {
            TurnOnSwitch();
        }
    }


    void TurnOnSwitch()
    {
        isSwitchOn = true;

        // Destroy laser object an toàn
        DisableAndDestroyLaser(laserObject);
        DisableAndDestroyLaser(laserObject_02);
        DisableAndDestroyLaser(laserObject_03);

        // Animation công tắc
        if (animator != null)
            animator.SetBool("isOn", true);

        // Âm thanh
        if (switchSound != null && audioSource != null)
            audioSource.PlayOneShot(switchSound);
    }

    void DisableAndDestroyLaser(GameObject laser)
    {
        if (laser != null)
        {
            StartCoroutine(DestroyAtEndOfFrame(laser));
        }
    }

    IEnumerator DestroyAtEndOfFrame(GameObject obj)
    {
        yield return new WaitForEndOfFrame();
        Destroy(obj);
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
