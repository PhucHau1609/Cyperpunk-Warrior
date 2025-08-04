using System.Collections.Generic;
using UnityEngine;

public class EquipmentConditionChecker : MonoBehaviour
{
    public static EquipmentConditionChecker Instance;

    [SerializeField] private EquipmentSlot[] equipmentSlots;
    [SerializeField] private ItemCode requiredArtefact;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public bool IsConditionMet()
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
    }


}
