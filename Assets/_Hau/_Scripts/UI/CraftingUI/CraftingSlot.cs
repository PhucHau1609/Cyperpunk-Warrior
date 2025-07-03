using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CraftingSlot : MonoBehaviour, IDropHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image icon;
    public ItemInventory currentItem; // chỉ là bản tạm

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector3 originalPosition;

    private GameObject draggingVisual;


    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        BtnItemInventory dragged = eventData.pointerDrag?.GetComponent<BtnItemInventory>();
        if (dragged == null || dragged.ItemInventory == null) return;

        ItemInventory sourceItem = dragged.ItemInventory;

        // Clone bản tạm để gán vào slot
        ItemInventory cloneItem = new ItemInventory(sourceItem.ItemProfileSO, 1);

        if (!sourceItem.Deduct(1)) return;

        if (sourceItem.itemCount <= 0)
            InventoryManager.Instance.RemoveItem(sourceItem);

        ObserverManager.Instance.PostEvent(EventID.InventoryChanged);

        this.SetItem(cloneItem); // bản clone để tránh side effect
    }

    public void SetItem(ItemInventory cloneItem)
    {
        this.currentItem = cloneItem;
        this.icon.sprite = cloneItem.ItemProfileSO.itemSprite;
        this.icon.color = Color.white;
    }


    public void Clear()
    {
        this.currentItem = null;
        this.icon.sprite = null;
        this.icon.color = new Color(1, 1, 1, 0); // ẩn nhưng vẫn raycast
    }




    public bool HasItem() => currentItem != null;

    // === DRAG TRẢ LẠI VỀ INVENTORY ===

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!HasItem()) return;

        // Tạo 1 bản UI ảo để drag (clone hình ảnh)
        draggingVisual = new GameObject("DraggingVisual", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        draggingVisual.transform.SetParent(transform.root); // UI root
        draggingVisual.transform.SetAsLastSibling(); // trên cùng

        RectTransform dragRect = draggingVisual.GetComponent<RectTransform>();
        dragRect.sizeDelta = icon.rectTransform.sizeDelta;
        dragRect.position = Input.mousePosition;

        Image dragImage = draggingVisual.GetComponent<Image>();
        dragImage.sprite = icon.sprite;
        dragImage.raycastTarget = false;

        CanvasGroup cg = draggingVisual.GetComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggingVisual != null)
            draggingVisual.transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggingVisual != null)
            Destroy(draggingVisual);

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
            Clear();
        }
    }

}


