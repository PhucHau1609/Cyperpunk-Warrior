using System.Collections.Generic;
using UnityEngine;

public class EquipmentConditionChecker : MonoBehaviour
{
    public static EquipmentConditionChecker Instance;

    [SerializeField] private EquipmentSlot[] equipmentSlots;
    [SerializeField] private ItemCode requiredArtefact;

    private bool lastState = false;


    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public bool IsConditionMet()
    {
        // Thu thập đủ 4 trang bị "Clothes" khác loại + có Artefact
        HashSet<EquipmentType> clothesEquipped = new();
        bool hasArtefact = false;

        // 1) Đếm số loại Clothes đang gắn ở các equipmentSlots
        if (equipmentSlots != null)
        {
            foreach (var slot in equipmentSlots)
            {
                if (slot == null) continue;
                if (!slot.HasItem()) continue;

                var item = slot.currentItem.ItemProfileSO;
                if (item == null) continue;

                // Đếm theo EquipmentType để tránh trùng loại (mũ/áo/quần/giày...)
                if (item.itemCode.ToString().StartsWith("Clothes"))
                {
                    clothesEquipped.Add(item.equipmentType);
                }
            }
        }

        // 2) Kiểm tra Artefact trong Inventory
        // - Nếu bạn có set "requiredArtefact" cụ thể -> bắt buộc đúng cái đó.
        // - Nếu để trống (None) -> chấp nhận bất kỳ Artefact nào còn > 0.
        if (requiredArtefact != ItemCode.NoName)
        {
            // Gợi ý: nên có helper trong InventoryManager, tạm dùng cách duyệt hiện có
            var inv = InventoryManager.Instance?.ItemInventory();
            if (inv != null)
            {
                foreach (var it in inv.ItemInventories)
                {
                    if (it?.ItemProfileSO == null) continue;
                    if (it.ItemProfileSO.itemCode == requiredArtefact && it.itemCount > 0)
                    {
                        hasArtefact = true;
                        break;
                    }
                }
            }
        }
        else
        {
            var inv = InventoryManager.Instance?.ItemInventory();
            if (inv != null)
            {
                foreach (var it in inv.ItemInventories)
                {
                    if (it?.ItemProfileSO == null) continue;
                    if (it.ItemProfileSO.itemCode.ToString().StartsWith("Artefacts") && it.itemCount > 0)
                    {
                        hasArtefact = true;
                        break;
                    }
                }
            }
        }

        bool ok = clothesEquipped.Count >= 4 && hasArtefact;

        // 3) Nếu trạng thái thay đổi -> bắn event (để UI/Nút kích hoạt biết mà enable/disable)
        if (ok != lastState)
        {
            lastState = ok;
            ObserverManager.Instance?.PostEvent(EventID.SkillPrereqChanged, ok);
            // Bạn có thể log nhẹ nếu cần:
            // Debug.Log($"[EquipmentConditionChecker] Prereq changed -> {(ok ? "READY" : "NOT READY")}");
        }

        return ok;
    }


    /*  public bool IsConditionMet()
      {
          HashSet<EquipmentType> clothesEquipped = new();
          bool hasArtefact = false;

          foreach (var slot in equipmentSlots)
          {
              if (slot == null)
              {
                  Debug.LogWarning("⚠️ Slot bị null trong danh sách!");
                  continue;
              }

              if (!slot.HasItem())
              {
                  Debug.Log($"🔲 Slot {slot.name} đang trống.");
                  continue;
              }

              var item = slot.currentItem.ItemProfileSO;
              if (item == null)
              {
                  Debug.LogWarning($"⚠️ Slot {slot.name} có item null.");
                  continue;
              }

              //Debug.Log($"🔍 Slot {slot.name} chứa item: {item.itemCode}");

              if (item.itemCode.ToString().StartsWith("Clothes"))
              {
                  clothesEquipped.Add(item.equipmentType);
              }
          }

          //✅ Check artefact từ inventory thay vì slot
          var inventory = InventoryManager.Instance.ItemInventory();
          foreach (var item in inventory.ItemInventories)
          {
              if (item.ItemProfileSO.itemCode.ToString().StartsWith("Artefacts") && item.itemCount > 0)
              {
                  hasArtefact = true;
                  //Debug.Log($"🧪 Tìm thấy Artefact: {item.ItemProfileSO.itemCode} trong inventory");
                  break;
              }
          }

          Debug.Log($"🧥 Số clothes: {clothesEquipped.Count}, Artefact: {hasArtefact}");
          return clothesEquipped.Count >= 4 && hasArtefact;




      }*/


}
