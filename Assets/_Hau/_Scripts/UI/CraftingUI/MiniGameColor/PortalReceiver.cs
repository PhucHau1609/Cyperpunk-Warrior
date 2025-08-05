using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class PortalReceiver : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    public SpriteRenderer portalRenderer;
    public Animator animator;

    public EnergyType currentEnergy;

    public event Action<PortalReceiver, EnergyType> OnEnergySet;

    private bool isReceiverEnergy = false;

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
        if (currentEnergy != energy) return;
        if (isReceiverEnergy)
        {
            //Debug.Log("Portal da co energy core");
            return;
        }

            currentEnergy = energy;

        // Trigger animation
        switch (energy)
        {
            case EnergyType.Orange:
                animator?.SetTrigger("Orange");
                InventoryManager.Instance.RemoveItem(ItemCode.UpgradeItem_3, 1);
                isReceiverEnergy = true;
                break;
            case EnergyType.Blue:
                animator?.SetTrigger("Blue");
                InventoryManager.Instance.RemoveItem(ItemCode.UpgradeItem_1, 1);
                isReceiverEnergy = true;
                break;
            case EnergyType.Purple:
                animator?.SetTrigger("Purple");
                InventoryManager.Instance.RemoveItem(ItemCode.UpgradeItem_5, 1);
                isReceiverEnergy = true;
                break;
        }

        OnEnergySet?.Invoke(this, energy); // ✅ Thông báo cho tracker
    }



  /*  public void SetEnergy(EnergyType energy)
    {
        // Nếu portal đã có energy rồi thì không cho gắn thêm
        if (currentEnergy != EnergyType.None)
        {
            Debug.Log("Portal already has an energy assigned: " + currentEnergy);
            return;
        }

        // Nếu energy truyền vào là None thì cũng bỏ qua
        if (energy == EnergyType.None) return;

        // Gán energy mới
        currentEnergy = energy;

        // Trigger animation và trừ item (chỉ lúc gắn thành công)
        switch (energy)
        {
            case EnergyType.Orange:
                animator?.SetTrigger("Orange");
                InventoryManager.Instance.RemoveItem(ItemCode.UpgradeItem_3, 1);
                break;
            case EnergyType.Blue:
                animator?.SetTrigger("Blue");
                InventoryManager.Instance.RemoveItem(ItemCode.UpgradeItem_1, 1);
                break;
            case EnergyType.Purple:
                animator?.SetTrigger("Purple");
                InventoryManager.Instance.RemoveItem(ItemCode.UpgradeItem_5, 1);
                break;
        }

        OnEnergySet?.Invoke(this, energy);
    }
*/

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

public enum EnergyType
{
    None,    // ✅ THÊM GIÁ TRỊ NÀY
    Orange,
    Blue,
    Purple
}
