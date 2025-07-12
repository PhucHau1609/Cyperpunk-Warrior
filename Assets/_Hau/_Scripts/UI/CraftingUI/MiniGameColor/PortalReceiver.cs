using UnityEngine;
using UnityEngine.EventSystems;

public class PortalReceiver : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    public SpriteRenderer portalRenderer;
    public Animator animator;

    public EnergyType currentEnergy;

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Chuột trái
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null)
            {
                if (hit.collider.gameObject == this.gameObject)
                {
                    // Bắt đúng object -> bật UI
                    EnergyCoreInventoryUI.Instance.ShowUI(); // hoặc ToggleUI()
                }
            }
        }
    }


    public void OnDrop(PointerEventData eventData)
    {
        // ✅ Lấy thông tin item được kéo
        BtnItemInventory draggedItem = eventData.pointerDrag?.GetComponent<BtnItemInventory>();
        if (draggedItem == null) return;

        var item = draggedItem.ItemInventory;
        if (item == null || item.ItemProfileSO == null) return;

        ApplyEnergyType(item.ItemProfileSO.itemCode);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // ❓ Khi click có thể hiện menu chọn năng lượng, ở đây ví dụ toggle nhanh
        CycleEnergyType();
    }

    private void ApplyEnergyType(ItemCode itemCode)
    {
        switch (itemCode)
        {
            case ItemCode.UpgradeItem_3:
                SetEnergy(EnergyType.Orange);
                break;
            case ItemCode.UpgradeItem_1:
                SetEnergy(EnergyType.Blue);
                break;
            case ItemCode.UpgradeItem_5:
                SetEnergy(EnergyType.Purple);
                break;
            default:
                Debug.LogWarning("Unknown energy item code: " + itemCode);
                break;
        }
    }


    public void SetEnergy(EnergyType energy)
    {
        currentEnergy = energy;

        // 👉 Đổi màu hoặc trigger animation
        switch (energy)
        {
            case EnergyType.Orange:
                animator?.SetTrigger("Orange");
                break;
            case EnergyType.Blue:
                animator?.SetTrigger("Blue");
                break;
            case EnergyType.Purple:
                animator?.SetTrigger("Purple");
                break;
        }
    }

    private void CycleEnergyType()
    {
        SetEnergy((EnergyType)(((int)currentEnergy + 1) % 3));
    }

    public void ReceiveItem(ItemInventory item)
    {
        if (item == null || item.ItemProfileSO == null) return;
        ApplyEnergyType(item.ItemProfileSO.itemCode);
    }

}

public enum EnergyType { Orange, Blue, Purple }

