using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CraftingSlot : MonoBehaviour, IDropHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI References")]
    [SerializeField] private Image backgroundImage; // Background của slot - KHÔNG thay đổi
    [SerializeField] private Image iconImage;       // Icon của item - chỉ thay đổi cái này

    public ItemInventory currentItem; // chỉ là bản tạm

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector3 originalPosition;

    [Header("Drag Settings")]
    private GameObject draggingVisual;
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private float dragSizeScale = 0.8f;

    public static event System.Action OnSlotChanged;


    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // ✅ Auto-setup nếu chưa assign
        SetupImageReferences();
    }

    // ✅ Tự động setup image references
    private void SetupImageReferences()
    {
        // Nếu chưa assign, tự động tìm
        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>(); // Image chính của GameObject
        }

        if (iconImage == null)
        {
            // Tìm child object có tên "Icon" hoặc "ItemIcon"
            Transform iconTransform = transform.Find("Icon");
            if (iconTransform == null)
                iconTransform = transform.Find("ItemIcon");

            if (iconTransform != null)
            {
                iconImage = iconTransform.GetComponent<Image>();
            }
            else
            {
                // Tạo icon image mới nếu không tìm thấy
                CreateIconImage();
            }
        }
    }

    // ✅ Tạo icon image mới
    private void CreateIconImage()
    {
        GameObject iconObj = new GameObject("ItemIcon", typeof(RectTransform), typeof(Image));
        iconObj.transform.SetParent(transform, false);

        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.sizeDelta = Vector2.zero;
        iconRect.anchoredPosition = Vector2.zero;

        iconImage = iconObj.GetComponent<Image>();
        iconImage.raycastTarget = false; // Không block raycasts
        iconImage.color = new Color(1, 1, 1, 0); // Trong suốt ban đầu
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

        // Nếu slot đã có item, trả về inventory
        if (HasItem())
        {
            InventoryManager.Instance.AddItem(currentItem.ItemProfileSO.itemCode, 1);
            ObserverManager.Instance.PostEvent(EventID.InventoryChanged);
        }

        // ✅ FIX: Sử dụng InventoryManager.RemoveItem thay vì thao tác trực tiếp
        if (!InventoryManager.Instance.ItemInventory().RemoveItem(new ItemInventory(sourceItem.ItemProfileSO, 1)))
        {
            return;
        }

        // Clone item để gán vào slot
        ItemInventory cloneItem = new ItemInventory(sourceItem.ItemProfileSO, 1);
        this.SetItem(cloneItem);

        // Trigger UI update
        ObserverManager.Instance.PostEvent(EventID.InventoryChanged);
    }

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

        OnSlotChanged?.Invoke();

    }

    // ✅ Cập nhật visual - CHỈ thay đổi iconImage, GIỮ NGUYÊN backgroundImage
    private void UpdateVisual()
    {
        if (iconImage == null)
        {
            Debug.LogError("CraftingSlot: iconImage is null! Please assign it in inspector.");
            return;
        }

        if (currentItem != null && currentItem.ItemProfileSO != null)
        {
            // Chỉ thay đổi icon image
            iconImage.sprite = currentItem.ItemProfileSO.itemSprite;
            iconImage.color = Color.white; // Hiển thị
        }
        else
        {
            // Ẩn icon khi không có item
            iconImage.sprite = null;
            iconImage.color = new Color(1, 1, 1, 0); // Trong suốt
        }
    }

    public void Clear()
    {
        this.currentItem = null;
        UpdateVisual();

        OnSlotChanged?.Invoke();

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
        {
            draggingVisual.transform.position = Input.mousePosition;
        }
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

    // ✅ Tạo visual drag - FIXED VERSION
    private void CreateDraggingVisual()
    {
        if (!HasItem()) return;

        draggingVisual = new GameObject("DraggingVisual", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));

        // 🔥 FIX 1: Tìm Canvas có sorting order cao nhất
        Canvas canvasToUse = targetCanvas;
        if (canvasToUse == null)
        {
            // Tìm canvas với sorting order cao nhất
            Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            int highestOrder = -1;
            foreach (Canvas canvas in allCanvases)
            {
                if (canvas.sortingOrder > highestOrder)
                {
                    highestOrder = canvas.sortingOrder;
                    canvasToUse = canvas;
                }
            }

            // Fallback: tìm canvas gần nhất
            if (canvasToUse == null)
            {
                canvasToUse = GetComponentInParent<Canvas>();
            }
        }

        if (canvasToUse != null)
        {
            draggingVisual.transform.SetParent(canvasToUse.transform, false);
        }
        else
        {
            draggingVisual.transform.SetParent(transform.root, false);
        }

        // 🔥 FIX 2: Đảm bảo render order cao nhất
        draggingVisual.transform.SetAsLastSibling();

        RectTransform dragRect = draggingVisual.GetComponent<RectTransform>();

        // 🔥 FIX 3: ĐÂY LÀ VỀN ĐỀ CHÍNH - FIX SIZE CALCULATION
        Vector2 baseSize = Vector2.zero;

        if (iconImage != null)
        {
            baseSize = iconImage.rectTransform.sizeDelta;
        }

        if (backgroundImage != null)
        {
            if (baseSize.magnitude <= 0)
                baseSize = backgroundImage.rectTransform.sizeDelta;
        }

        if (baseSize.magnitude <= 0)
            baseSize = rectTransform.sizeDelta;

        // 🔥 FIX 4: Fallback to default size nếu vẫn = 0
        if (baseSize.magnitude <= 0)
        {
            baseSize = new Vector2(64, 64); // Default size
        }

        Vector2 finalSize = baseSize * dragSizeScale;
        dragRect.sizeDelta = finalSize;

        // 🔥 FIX 5: ĐẢM BẢO POSITION ĐÚNG
        // Set anchor và pivot về center
        dragRect.anchorMin = Vector2.one * 0.5f;
        dragRect.anchorMax = Vector2.one * 0.5f;
        dragRect.pivot = Vector2.one * 0.5f;

        // Set position using screen coordinates
        Vector2 screenPos = Input.mousePosition;
        dragRect.position = screenPos;

        Image dragImage = draggingVisual.GetComponent<Image>();
        dragImage.sprite = currentItem.ItemProfileSO.itemSprite;
        dragImage.raycastTarget = false;

        CanvasGroup cg = draggingVisual.GetComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
        cg.alpha = 0.8f;

        // 🔥 FIX 6: Thêm Canvas component để đảm bảo render order
        Canvas dragCanvas = draggingVisual.AddComponent<Canvas>();
        dragCanvas.overrideSorting = true;
        dragCanvas.sortingOrder = 1000; // Rất cao để đảm bảo render trên cùng

        // 🔥 FIX 7: Thêm GraphicRaycaster nếu cần
        if (draggingVisual.GetComponent<GraphicRaycaster>() == null)
        {
            draggingVisual.AddComponent<GraphicRaycaster>();
        }
    }
}

/*public class CraftingSlot : MonoBehaviour, IDropHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI References")]
    [SerializeField] private Image backgroundImage; // Background của slot - KHÔNG thay đổi
    [SerializeField] private Image iconImage;       // Icon của item - chỉ thay đổi cái này

    public ItemInventory currentItem; // chỉ là bản tạm

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector3 originalPosition;

    [Header("Drag Settings")]
    private GameObject draggingVisual;
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private float dragSizeScale = 0.8f;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // ✅ Auto-setup nếu chưa assign
        SetupImageReferences();
    }

    // ✅ Tự động setup image references
    private void SetupImageReferences()
    {
        //Debug.Log("[CraftingSlot] Setting up image references...");

        // Nếu chưa assign, tự động tìm
        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>(); // Image chính của GameObject
            //Debug.Log($"[CraftingSlot] BackgroundImage assigned: {backgroundImage != null}, Size: {backgroundImage?.rectTransform.sizeDelta}");
        }

        if (iconImage == null)
        {
            // Tìm child object có tên "Icon" hoặc "ItemIcon"
            Transform iconTransform = transform.Find("Icon");
            if (iconTransform == null)
                iconTransform = transform.Find("ItemIcon");

            if (iconTransform != null)
            {
                iconImage = iconTransform.GetComponent<Image>();
                //Debug.Log($"[CraftingSlot] IconImage found: {iconImage != null}, Size: {iconImage?.rectTransform.sizeDelta}");
            }
            else
            {
                // Tạo icon image mới nếu không tìm thấy
                //Debug.Log("[CraftingSlot] Creating new icon image...");
                CreateIconImage();
            }
        }

        //Debug.Log($"[CraftingSlot] Setup complete - Background: {backgroundImage != null}, Icon: {iconImage != null}");
    }

    // ✅ Tạo icon image mới
    private void CreateIconImage()
    {
        GameObject iconObj = new GameObject("ItemIcon", typeof(RectTransform), typeof(Image));
        iconObj.transform.SetParent(transform, false);

        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.sizeDelta = Vector2.zero;
        iconRect.anchoredPosition = Vector2.zero;

        iconImage = iconObj.GetComponent<Image>();
        iconImage.raycastTarget = false; // Không block raycasts
        iconImage.color = new Color(1, 1, 1, 0); // Trong suốt ban đầu
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

        // Nếu slot đã có item, trả về inventory
        if (HasItem())
        {
            InventoryManager.Instance.AddItem(currentItem.ItemProfileSO.itemCode, 1);
            ObserverManager.Instance.PostEvent(EventID.InventoryChanged);
        }

        // ✅ FIX: Sử dụng InventoryManager.RemoveItem thay vì thao tác trực tiếp
        if (!InventoryManager.Instance.ItemInventory().RemoveItem(new ItemInventory(sourceItem.ItemProfileSO, 1)))
        {
            return;
        }

        // Clone item để gán vào slot
        ItemInventory cloneItem = new ItemInventory(sourceItem.ItemProfileSO, 1);
        this.SetItem(cloneItem);

        // Trigger UI update
        ObserverManager.Instance.PostEvent(EventID.InventoryChanged);
    }

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

    // ✅ Cập nhật visual - CHỈ thay đổi iconImage, GIỮ NGUYÊN backgroundImage
    private void UpdateVisual()
    {
        if (iconImage == null)
        {
            Debug.LogError("CraftingSlot: iconImage is null! Please assign it in inspector.");
            return;
        }

        if (currentItem != null && currentItem.ItemProfileSO != null)
        {
            // Chỉ thay đổi icon image
            iconImage.sprite = currentItem.ItemProfileSO.itemSprite;
            iconImage.color = Color.white; // Hiển thị
        }
        else
        {
            // Ẩn icon khi không có item
            iconImage.sprite = null;
            iconImage.color = new Color(1, 1, 1, 0); // Trong suốt
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
        {
            draggingVisual.transform.position = Input.mousePosition;

            // 🔥 DEBUG: Kiểm tra tình trạng drag visual
            //DebugDragVisual();
        }
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

    // ✅ Tạo visual drag - FIXED VERSION
    private void CreateDraggingVisual()
    {
        if (!HasItem()) return;

        //Debug.Log($"[CraftingSlot] Creating drag visual for item: {currentItem.ItemProfileSO.itemCode}");

        draggingVisual = new GameObject("DraggingVisual", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));

        // 🔥 FIX 1: Tìm Canvas có sorting order cao nhất
        Canvas canvasToUse = targetCanvas;
        if (canvasToUse == null)
        {
            // Tìm canvas với sorting order cao nhất
            Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            int highestOrder = -1;
            foreach (Canvas canvas in allCanvases)
            {
                if (canvas.sortingOrder > highestOrder)
                {
                    highestOrder = canvas.sortingOrder;
                    canvasToUse = canvas;
                }
            }

            // Fallback: tìm canvas gần nhất
            if (canvasToUse == null)
            {
                canvasToUse = GetComponentInParent<Canvas>();
            }
        }

        if (canvasToUse != null)
        {
            draggingVisual.transform.SetParent(canvasToUse.transform, false);
        }
        else
        {
            //Debug.LogError("CraftingSlot: Cannot find Canvas for dragging visual!");
            draggingVisual.transform.SetParent(transform.root, false);
        }

        // 🔥 FIX 2: Đảm bảo render order cao nhất
        draggingVisual.transform.SetAsLastSibling();

        RectTransform dragRect = draggingVisual.GetComponent<RectTransform>();

        // 🔥 FIX 3: ĐÂY LÀ VỀN ĐỀ CHÍNH - FIX SIZE CALCULATION
        Vector2 baseSize = Vector2.zero;

        //Debug.Log($"[CraftingSlot] IconImage null? {iconImage == null}");
        if (iconImage != null)
        {
           // Debug.Log($"[CraftingSlot] IconImage size: {iconImage.rectTransform.sizeDelta}");
            baseSize = iconImage.rectTransform.sizeDelta;
        }

        //Debug.Log($"[CraftingSlot] BackgroundImage null? {backgroundImage == null}");
        if (backgroundImage != null)
        {
            //Debug.Log($"[CraftingSlot] BackgroundImage size: {backgroundImage.rectTransform.sizeDelta}");
            if (baseSize.magnitude <= 0)
                baseSize = backgroundImage.rectTransform.sizeDelta;
        }

        //Debug.Log($"[CraftingSlot] RectTransform size: {rectTransform.sizeDelta}");
        if (baseSize.magnitude <= 0)
            baseSize = rectTransform.sizeDelta;

        // 🔥 FIX 4: Fallback to default size nếu vẫn = 0
        if (baseSize.magnitude <= 0)
        {
            baseSize = new Vector2(64, 64); // Default size
            //Debug.LogWarning("[CraftingSlot] All sizes are 0, using default size: " + baseSize);
        }

        Vector2 finalSize = baseSize * dragSizeScale;
        dragRect.sizeDelta = finalSize;

        // 🔥 FIX 5: ĐẢM BẢO POSITION ĐÚNG
        // Set anchor và pivot về center
        dragRect.anchorMin = Vector2.one * 0.5f;
        dragRect.anchorMax = Vector2.one * 0.5f;
        dragRect.pivot = Vector2.one * 0.5f;

        // Set position using screen coordinates
        Vector2 screenPos = Input.mousePosition;
        dragRect.position = screenPos;

        //Debug.Log($"[CraftingSlot] Final drag size: {finalSize}, Screen pos: {screenPos}");

        Image dragImage = draggingVisual.GetComponent<Image>();
        dragImage.sprite = currentItem.ItemProfileSO.itemSprite;
        dragImage.raycastTarget = false;

        CanvasGroup cg = draggingVisual.GetComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
        cg.alpha = 0.8f;

        // 🔥 FIX 6: Thêm Canvas component để đảm bảo render order
        Canvas dragCanvas = draggingVisual.AddComponent<Canvas>();
        dragCanvas.overrideSorting = true;
        dragCanvas.sortingOrder = 1000; // Rất cao để đảm bảo render trên cùng

        // 🔥 FIX 7: Thêm GraphicRaycaster nếu cần
        if (draggingVisual.GetComponent<GraphicRaycaster>() == null)
        {
            draggingVisual.AddComponent<GraphicRaycaster>();
        }

        //Debug.Log($"[CraftingSlot] Drag visual created - Size: {dragRect.sizeDelta}, Position: {dragRect.position}, Alpha: {cg.alpha}");
    }
}*/
