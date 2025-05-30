using UnityEngine;

public class SwitchController : MonoBehaviour
{
    [Header("Tham chiếu tới Laser và âm thanh")]
    public GameObject laserObject;         // Đối tượng laser để ẩn
    public AudioClip switchSound;          // Âm thanh khi bật công tắc

    private AudioSource audioSource;       // Phát âm thanh
    private Animator animator;             // Điều khiển animation công tắc

    private bool isPlayerInRange = false;  // Người chơi đang trong vùng công tắc
    private bool isSwitchOn = false;       // Trạng thái công tắc (bắt đầu là tắt)

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
        if (isPlayerInRange && !isSwitchOn && Input.GetKeyDown(KeyCode.Return))
        {
            TurnOnSwitch();
        }
    }

    void TurnOnSwitch()
    {
        isSwitchOn = true;

        // Ẩn laser
        if (laserObject != null)
            laserObject.SetActive(false);

        // Chạy animation chuyển công tắc
        if (animator != null)
            animator.SetBool("isOn", true);

        // Phát âm thanh
        if (switchSound != null && audioSource != null)
            audioSource.PlayOneShot(switchSound);
    }

    // Khi người chơi vào vùng công tắc
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            isPlayerInRange = true;
    }

    // Khi người chơi rời vùng công tắc
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            isPlayerInRange = false;
    }
}
