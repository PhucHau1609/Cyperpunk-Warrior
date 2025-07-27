using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EquipmentSlot : MonoBehaviour, IDropHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Image hintImage; // <-- ảnh gợi ý: icon mũ, áo...


    public ItemInventory currentItem;
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private float dragSizeScale = 0.8f;

    private CanvasGroup canvasGroup;
    private GameObject draggingVisual;
    private RectTransform rectTransform;

    [SerializeField] private EquipmentType slotType = EquipmentType.None;


    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (iconImage == null)
        {
            Transform icon = transform.Find("Icon") ?? transform.Find("ItemIcon");
            if (icon != null) iconImage = icon.GetComponent<Image>();
        }

        if (iconImage != null) iconImage.color = new Color(1, 1, 1, 0);
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject draggedObject = eventData.pointerDrag;
        if (draggedObject == null) return;

        BtnItemInventory btnItem = draggedObject.GetComponent<BtnItemInventory>();
        if (btnItem != null && btnItem.ItemInventory != null)
        {
            HandleDropFromInventory(btnItem);
        }
    }

    private void HandleDropFromInventory(BtnItemInventory btnItem)
    {
        if (btnItem == null || btnItem.ItemInventory == null || btnItem.ItemInventory.ItemProfileSO == null)
        {
            Debug.LogError("❌ Item null hoặc thiếu profile!");
            return;
        }

        ItemInventory sourceItem = btnItem.ItemInventory;
        var profile = sourceItem.ItemProfileSO;

        // 🔒 Kiểm tra loại trang bị có hợp lệ không
        if (profile.equipmentType != slotType)
        {
            Debug.LogWarning($"🚫 Không thể gắn {profile.itemName} vào slot {slotType}");
            return;
        }

        // 🔁 Nếu có item cũ → hoàn về inventory
        if (HasItem())
        {
            InventoryManager.Instance.AddItem(currentItem.ItemProfileSO.itemCode, 1);
            ObserverManager.Instance.PostEvent(EventID.InventoryChanged);
        }

        // ✅ Trừ item từ inventory
        bool removed = InventoryManager.Instance.ItemInventory().RemoveItem(new ItemInventory(profile, 1));
        if (!removed)
        {
            Debug.LogWarning("⚠️ Không thể remove item từ inventory.");
            return;
        }

        currentItem = new ItemInventory(profile, 1);
        UpdateVisual();

        ObserverManager.Instance.PostEvent(EventID.InventoryChanged);
    }


    public bool HasItem()
    {
        return currentItem != null && currentItem.ItemProfileSO != null;
    }





    private void UpdateVisual()
    {
        if (iconImage == null) return;

        bool hasItem = currentItem != null && currentItem.ItemProfileSO != null;

        if (hasItem)
        {
            iconImage.sprite = currentItem.ItemProfileSO.itemSprite;
            iconImage.color = Color.white;

            if (hintImage != null)
                hintImage.gameObject.SetActive(false); // ẩn gợi ý
        }
        else
        {
            iconImage.sprite = null;
            iconImage.color = new Color(1, 1, 1, 0); // trong suốt

            if (hintImage != null)
                hintImage.gameObject.SetActive(true); // hiện gợi ý
        }
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!HasItem())
        {
            Debug.Log("⚠️ Không thể kéo từ EquipmentSlot vì đang trống.");
            return;
        }

        canvasGroup.blocksRaycasts = false;
        CreateDraggingVisual();
    }


    public void OnDrag(PointerEventData eventData)
    {
        if (draggingVisual != null)
        {
            draggingVisual.transform.position = Input.mousePosition;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!HasItem())
        {
            Debug.Log("⚠️ Không thể kéo từ EquipmentSlot vì đang trống.");
            return;
        }

        canvasGroup.blocksRaycasts = true;

        if (draggingVisual != null)
        {
            Destroy(draggingVisual);
            draggingVisual = null;
        }

        GameObject target = eventData.pointerEnter;
        bool returnedToInventory = false;

        if (target != null && target.GetComponentInParent<NewInventoryUI>() != null)
        {
            InventoryManager.Instance.AddItem(currentItem.ItemProfileSO.itemCode, 1);
            ObserverManager.Instance.PostEvent(EventID.InventoryChanged);
            returnedToInventory = true;
        }

        if (returnedToInventory)
        {
            currentItem = null;
            UpdateVisual();
        }
    }

    private void CreateDraggingVisual()
    {
        if (!HasItem()) return;


        draggingVisual = new GameObject("DraggingVisual", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));

        Canvas canvasToUse = targetCanvas ?? GetComponentInParent<Canvas>();
        draggingVisual.transform.SetParent(canvasToUse?.transform ?? transform.root, false);
        draggingVisual.transform.SetAsLastSibling();

        RectTransform dragRect = draggingVisual.GetComponent<RectTransform>();
        dragRect.sizeDelta = iconImage.rectTransform.sizeDelta * dragSizeScale;
        dragRect.position = Input.mousePosition;

        Image dragImage = draggingVisual.GetComponent<Image>();
        dragImage.sprite = currentItem.ItemProfileSO.itemSprite;
        dragImage.raycastTarget = false;

        CanvasGroup cg = draggingVisual.GetComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
        cg.alpha = 0.8f;

        var dragCanvas = draggingVisual.AddComponent<Canvas>();
        dragCanvas.overrideSorting = true;
        dragCanvas.sortingOrder = 1000;
    }
}
