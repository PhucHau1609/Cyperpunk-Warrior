using UnityEngine;

public class HealHandler : IItemUseHandler
{
    public void Use(ItemInventory itemInventory)
    {
        var profile = itemInventory.ItemProfileSO;
        CharacterController2D controller = GameObject.FindFirstObjectByType<CharacterController2D>();
        playerHealth player = GameObject.FindFirstObjectByType<playerHealth>();

        if (controller == null || player == null)
        {
            Debug.LogWarning("Không tìm thấy player để hồi máu");
            return;
        }

        if (controller.life >= controller.maxLife)
        {
            Debug.Log("🔋 Máu đã đầy, không thể dùng bình máu.");
            return;
        }

        player.Heal(profile.healAmount);
        HauSoundManager.Instance.SpawnSound(Vector3.zero, SoundName.HealSound);
        InventoryManager.Instance.RemoveItem(profile.itemCode, 1);
    }
}
