using UnityEngine;

public class HealOnPickup : MonoBehaviour
{
    [Header("Cấu hình hồi máu")]
    public float healAmount = 1f;
    public AudioClip healSound;
    public ItemsDropDespawn itemsDropDespawn;


    void OnTriggerEnter2D(Collider2D other)
    {

        playerHealth player = other.GetComponent<playerHealth>();
        CharacterController2D controller = other.GetComponent<CharacterController2D>();
        if (player == null || controller == null) return;

        //Debug.Log($"[HealOnPickup] OnTriggerEnter by: {other.name}");



        if (controller.life < controller.maxLife)
        {
            // Hồi máu
            player.Heal(healAmount);

            // Âm thanh
            if (healSound != null) PlayHealSound();

            Destroy(gameObject); // Item biến mất sau khi hồi máu
        }
        else
        {
            // Thêm vào inventory nếu máu đầy
            if (itemsDropDespawn != null)
            {
                itemsDropDespawn.DoDespawn(); // Gọi hệ thống object pool để despawn + add inventory
            }
        }
    }

    public void PlayHealSound()
    {
        AudioSource.PlayClipAtPoint(healSound, transform.position);

    }
}
