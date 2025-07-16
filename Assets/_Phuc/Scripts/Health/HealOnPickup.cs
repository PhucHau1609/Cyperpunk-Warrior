using UnityEngine;

public class HealOnPickup : MonoBehaviour
{
    [Header("Cấu hình hồi máu")]
    public float healAmount = 1f;
    public AudioClip healSound;

    private bool collected = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;

        // Tìm các component cần thiết
        playerHealth player = other.GetComponent<playerHealth>();
        CharacterController2D controller = other.GetComponent<CharacterController2D>();

        // Đảm bảo đúng đối tượng và chưa đủ máu
        if (player != null && controller != null && controller.life < controller.maxLife)
        {
            collected = true;

            // Hồi máu
            player.Heal(healAmount);

            // Âm thanh hồi máu
            if (healSound != null)
            {
                AudioSource.PlayClipAtPoint(healSound, transform.position);
            }

            // Huỷ vật phẩm
            Destroy(gameObject);
        }
    }
}
