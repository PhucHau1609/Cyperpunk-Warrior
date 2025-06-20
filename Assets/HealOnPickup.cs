using UnityEngine;

public class HealOnPickup : MonoBehaviour
{
    public float healAmount = 1f;
    public AudioClip healSound;

    private bool collected = false;

   /* void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;

        // Lấy Component đúng
        CharacterController2D player = other.GetComponent<CharacterController2D>();

        // Nếu có player và máu chưa đầy
        if (player != null && player.life < player.maxLife)
        {
            collected = true;

            // Hồi máu (CharacterController2D đã có sẵn hàm Heal)
            player.Heal(healAmount);

            // Phát âm thanh nếu có
            if (healSound != null)
            {
                GameObject audioObj = new GameObject("HealSoundTemp");
                AudioSource tempAudio = audioObj.AddComponent<AudioSource>();
                tempAudio.PlayOneShot(healSound);
                Destroy(audioObj, healSound.length);
            }

            // Xoá vật phẩm
            Destroy(gameObject);
        }
    }*/
}
