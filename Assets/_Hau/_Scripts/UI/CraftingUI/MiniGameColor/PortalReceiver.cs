using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class PortalReceiver : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    [Header("Render/Anim")]
    public SpriteRenderer portalRenderer;
    public Animator animator;

    [Header("Portal Settings")]
    [Tooltip("Loại năng lượng mà portal này chấp nhận")]
    public EnergyType portalType = EnergyType.None;

    [Tooltip("VFX đã đặt sẵn trong scene (đang tắt). Sẽ bật khi nhận đúng năng lượng.")]
    public GameObject presetVfxObject; // gán object VFX có sẵn, đang SetActive(false)

    [Tooltip("Điểm bật UI khi click vào portal (giữ nguyên logic cũ)")]
    public bool openEnergyUIOnClick = true;

    [Header("State")]
    public EnergyType currentEnergy = EnergyType.None;
    private bool isReceiverEnergy = false;

    public event Action<PortalReceiver, EnergyType> OnEnergySet;

    void Update()
    {
        // Click chuột trái vào đúng portal -> bật UI chọn năng lượng (nếu bật)
        if (openEnergyUIOnClick && Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject == this.gameObject)
            {
                EnergyCoreInventoryUI.Instance.ShowUI();
            }
        }
    }

    // Kéo-thả item vào cổng
    public void OnDrop(PointerEventData eventData)
    {
        BtnItemInventory draggedItem = eventData.pointerDrag?.GetComponent<BtnItemInventory>();
        if (draggedItem == null) return;

        var item = draggedItem.ItemInventory;
        if (item == null || item.ItemProfileSO == null) return;

        ApplyEnergyType(item.ItemProfileSO.itemCode);
    }

    // Click để test nhanh (tuỳ chọn)
    public void OnPointerClick(PointerEventData eventData)
    {
        // Test: cố gắng set đúng loại của portal
        // SetEnergy(portalType);
    }

    // Map ItemCode -> EnergyType
    private void ApplyEnergyType(ItemCode itemCode)
    {
        switch (itemCode)
        {
            case ItemCode.UpgradeItem_6: SetEnergy(EnergyType.Orange); break;
            case ItemCode.UpgradeItem_1: SetEnergy(EnergyType.Blue); break;
            case ItemCode.UpgradeItem_5: SetEnergy(EnergyType.Purple); break;
            default:
                Debug.LogWarning("Unknown energy item code: " + itemCode);
                break;
        }
    }

    // Set năng lượng cho cổng + trừ item + chạy animation + BẬT VFX CÓ SẴN
    public void SetEnergy(EnergyType energy)
    {
        if (isReceiverEnergy) return;                 // đã gắn rồi
        if (energy != portalType) return;            // sai loại -> bỏ qua
        if (energy == EnergyType.None) return;

        currentEnergy = energy;

        // Trigger animation + trừ item tương ứng
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

        isReceiverEnergy = true;

        // ✅ Bật VFX đã đặt sẵn
        EnablePresetVFX();

        // Thông báo ra ngoài
        OnEnergySet?.Invoke(this, energy);
    }

    // Bật object VFX và play tất cả ParticleSystem con (nếu có)
    private void EnablePresetVFX()
    {
        if (presetVfxObject == null) return;

        if (!presetVfxObject.activeSelf)
            presetVfxObject.SetActive(true);

        // Nếu VFX là ParticleSystem, play lại để chắc chắn hiển thị
        var particles = presetVfxObject.GetComponentsInChildren<ParticleSystem>(true);
        foreach (var ps in particles)
        {
            ps.Clear(true);
            ps.Play(true);
        }
    }

    // API nhận item từ code (không cần thao tác kéo-thả)
    public void ReceiveItem(ItemInventory item)
    {
        if (item == null || item.ItemProfileSO == null) return;
        ApplyEnergyType(item.ItemProfileSO.itemCode);
    }
}

public enum EnergyType
{
    None,
    Orange,
    Blue,
    Purple
}
