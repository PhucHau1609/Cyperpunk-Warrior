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

    [Header("Drag Settings")]
    private GameObject draggingVisual;
    [SerializeField] private Canvas targetCanvas; // <-- thêm dòng này
    [SerializeField] private float dragSizeScale = 0.8f;



    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject draggedObject = eventData.pointerDrag;
        if (draggedObject == null) return;

        // ✅ Xử lý drop từ BtnItemInventory
        BtnItemInventory btnItem = draggedObject.GetComponent<BtnItemInventory>();
        if (btnItem != null && btnItem.ItemInventory != null)
        {
            HandleInventoryItemDrop(btnItem);
            return;
        }

        // ✅ Xử lý swap giữa các CraftingSlot
        CraftingSlot otherSlot = draggedObject.GetComponent<CraftingSlot>();
        if (otherSlot != null && otherSlot != this)
        {
            HandleCraftingSlotSwap(otherSlot);
            return;
        }
    }

    // ✅ Xử lý drop từ inventory - FIXED VERSION
    private void HandleInventoryItemDrop(BtnItemInventory btnItem)
    {
        ItemInventory sourceItem = btnItem.ItemInventory;

        //Debug.Log($"[CraftingSlot] HandleInventoryItemDrop - Source item: {sourceItem.ItemProfileSO.itemCode}, Count: {sourceItem.itemCount}");

        // Nếu slot đã có item, trả về inventory
        if (HasItem())
        {
            //Debug.Log($"[CraftingSlot] Slot already has item: {currentItem.ItemProfileSO.itemCode}, returning to inventory");
            InventoryManager.Instance.AddItem(currentItem.ItemProfileSO.itemCode, 1);
            ObserverManager.Instance.PostEvent(EventID.InventoryChanged);
        }

        // ✅ FIX: Sử dụng InventoryManager.RemoveItem thay vì thao tác trực tiếp
        // Điều này đảm bảo logic remove được xử lý đúng trong InventoryCtrl
        if (!InventoryManager.Instance.ItemInventory().RemoveItem(new ItemInventory(sourceItem.ItemProfileSO, 1)))
        {
            //Debug.LogError($"[CraftingSlot] Cannot remove item from inventory: {sourceItem.ItemProfileSO.itemCode}");
            return;
        }

        //Debug.Log($"[CraftingSlot] Successfully removed 1 item from inventory: {sourceItem.ItemProfileSO.itemCode}");

        // Clone item để gán vào slot
        ItemInventory cloneItem = new ItemInventory(sourceItem.ItemProfileSO, 1);
        this.SetItem(cloneItem);

        //Debug.Log($"[CraftingSlot] Item set to crafting slot: {cloneItem.ItemProfileSO.itemCode}");

        // Trigger UI update
        ObserverManager.Instance.PostEvent(EventID.InventoryChanged);
    }
    /* private void HandleInventoryItemDrop(BtnItemInventory btnItem)
     {
         ItemInventory sourceItem = btnItem.ItemInventory;

         // Nếu slot đã có item, trả về inventory
         if (HasItem())
         {
             InventoryManager.Instance.AddItem(currentItem.ItemProfileSO.itemCode, 1);
             ObserverManager.Instance.PostEvent(EventID.InventoryChanged);
         }

         // Clone item để gán vào slot
         ItemInventory cloneItem = new ItemInventory(sourceItem.ItemProfileSO, 1);

         if (!sourceItem.Deduct(1)) return;

         if (sourceItem.itemCount <= 0)
             InventoryManager.Instance.RemoveItem(sourceItem);

         ObserverManager.Instance.PostEvent(EventID.InventoryChanged);

         this.SetItem(cloneItem);
     }*/

    // ✅ Xử lý swap giữa các crafting slot
    private void HandleCraftingSlotSwap(CraftingSlot otherSlot)
    {
        ItemInventory tempItem = this.currentItem;
        this.SetItem(otherSlot.currentItem);
        otherSlot.SetItem(tempItem);

        // Cập nhật UI
        this.UpdateVisual();
        otherSlot.UpdateVisual();
    }

    public void SetItem(ItemInventory cloneItem)
    {
        this.currentItem = cloneItem;
        UpdateVisual();
    }

    // ✅ Cập nhật visual riêng biệt
    private void UpdateVisual()
    {
        if (currentItem != null && currentItem.ItemProfileSO != null)
        {
            this.icon.sprite = currentItem.ItemProfileSO.itemSprite;
            this.icon.color = Color.white;
        }
        else
        {
            this.icon.sprite = null;
            this.icon.color = new Color(1, 1, 1, 0);
        }
    }

    public void Clear()
    {
        this.currentItem = null;
        UpdateVisual();
    }

    public bool HasItem() => currentItem != null && currentItem.ItemProfileSO != null;

    // === DRAG TRẢ LẠI VỀ INVENTORY ===

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!HasItem()) return;

        // ✅ Tạo dragging visual giống như BtnItemInventory
        CreateDraggingVisual();

        // Set để có thể nhận drop
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggingVisual != null)
            draggingVisual.transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        // ✅ Destroy dragging visual
        if (draggingVisual != null)
        {
            Destroy(draggingVisual);
            draggingVisual = null;
        }

        GameObject target = eventData.pointerEnter;
        bool returnedToInventory = false;

        // ✅ Kiểm tra nếu thả vào inventory
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

    // ✅ Tạo visual drag giống BtnItemInventory

    private void CreateDraggingVisual()
    {
        if (!HasItem()) return;

        draggingVisual = new GameObject("DraggingVisual", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));

        // ✅ Gán vào canvas chỉ định thay vì FindFirstObjectByType
        if (targetCanvas != null)
        {
            draggingVisual.transform.SetParent(targetCanvas.transform, false);
        }
        else
        {
            Debug.LogWarning("CraftingSlot: targetCanvas is not assigned. Falling back to root.");
            draggingVisual.transform.SetParent(transform.root, false);
        }

        draggingVisual.transform.SetAsLastSibling();

        RectTransform dragRect = draggingVisual.GetComponent<RectTransform>();
        dragRect.sizeDelta = icon.rectTransform.sizeDelta * dragSizeScale;
        dragRect.position = Input.mousePosition;

        Image dragImage = draggingVisual.GetComponent<Image>();
        dragImage.sprite = icon.sprite;
        dragImage.raycastTarget = false;

        CanvasGroup cg = draggingVisual.GetComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
        cg.alpha = 0.8f;
    }
    /*private void CreateDraggingVisual()
    {
        if (!HasItem()) return;

        draggingVisual = new GameObject("DraggingVisual", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));

        // Set parent là Canvas root để hiển thị trên tất cả UI
        Canvas rootCanvas = FindFirstObjectByType<Canvas>();
        if (rootCanvas != null)
        {
            draggingVisual.transform.SetParent(rootCanvas.transform, false);
        }
        else
        {
            draggingVisual.transform.SetParent(transform.root, false);
        }

        draggingVisual.transform.SetAsLastSibling();

        RectTransform dragRect = draggingVisual.GetComponent<RectTransform>();
        dragRect.sizeDelta = icon.rectTransform.sizeDelta; // * với dragSizeScale như bên btniteminventory
        dragRect.position = Input.mousePosition;

        Image dragImage = draggingVisual.GetComponent<Image>();
        dragImage.sprite = icon.sprite;
        dragImage.raycastTarget = false;

        CanvasGroup cg = draggingVisual.GetComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
        cg.alpha = 0.8f;
    }*/
}

/*public class CraftingSlot : MonoBehaviour, IDropHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
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

}*/


