using UnityEngine;

public class HealOnPickup : MonoBehaviour
{
    public float healAmount = 1f;
    public AudioClip healSound;

    private bool collected = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;

        playerHealth player = other.GetComponent<playerHealth>();
        if (player != null)
        {
            // Nếu máu chưa đầy mới cho nhặt
            if (player.health < player.maxHealth)
            {
                player.Heal(healAmount);              // Hồi máu
                player.PlayHealEffect();              // Gọi hiệu ứng hồi phục (nếu có)
            }
            else
            {
                return; // Không nhặt nếu máu đầy
            }

            // Phát âm thanh bằng AudioSource tạm
            if (healSound != null)
            {
                GameObject audioObj = new GameObject("HealSoundTemp");
                AudioSource tempAudio = audioObj.AddComponent<AudioSource>();
                tempAudio.PlayOneShot(healSound);
                Destroy(audioObj, healSound.length);
            }

            collected = true;
            Destroy(gameObject); // Huỷ vật phẩm ngay
        }
    }
}
