using TMPro;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

public class CraftingResultSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI References")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI quantityText; // ✅ Hiển thị số lượng

    public ItemInventory currentItem;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private GameObject draggingVisual;
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private float dragSizeScale = 0.8f;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        SetupImageReferences();
    }

    private void SetupImageReferences()
    {
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();

        if (iconImage == null)
        {
            Transform iconTransform = transform.Find("Icon") ?? transform.Find("ItemIcon");
            if (iconTransform != null)
                iconImage = iconTransform.GetComponent<Image>();
            else
                CreateIconImage();
        }

        if (quantityText == null)
        {
            Transform quantityTransform = transform.Find("Quantity") ?? transform.Find("QuantityText");
            if (quantityTransform != null)
                quantityText = quantityTransform.GetComponent<TextMeshProUGUI>();
            else
                CreateQuantityText();
        }
    }

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
        iconImage.raycastTarget = false;
        iconImage.color = new Color(1, 1, 1, 0);
    }

    private void CreateQuantityText()
    {
        GameObject quantityObj = new GameObject("QuantityText", typeof(RectTransform), typeof(TextMeshProUGUI));
        quantityObj.transform.SetParent(transform, false);

        RectTransform quantityRect = quantityObj.GetComponent<RectTransform>();
        quantityRect.anchorMin = new Vector2(0.7f, 0f);
        quantityRect.anchorMax = new Vector2(1f, 0.3f);
        quantityRect.sizeDelta = Vector2.zero;
        quantityRect.anchoredPosition = Vector2.zero;

        quantityText = quantityObj.GetComponent<TextMeshProUGUI>();
        quantityText.text = "";
        quantityText.fontSize = 12;
        quantityText.color = Color.white;
        quantityText.alignment = TextAlignmentOptions.Center;
        quantityText.raycastTarget = false;
    }

    public void SetItem(ItemInventory item)
    {
        this.currentItem = item;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (iconImage == null) return;

        if (currentItem != null && currentItem.ItemProfileSO != null)
        {
            iconImage.sprite = currentItem.ItemProfileSO.itemSprite;
            iconImage.color = Color.white;

            // ✅ Hiển thị số lượng
            if (quantityText != null)
            {
                quantityText.text = currentItem.itemCount > 1 ? currentItem.itemCount.ToString() : "";
            }
        }
        else
        {
            iconImage.sprite = null;
            iconImage.color = new Color(1, 1, 1, 0);

            if (quantityText != null)
                quantityText.text = "";
        }
    }

    public void Clear()
    {
        this.currentItem = null;
        UpdateVisual();
    }

    public bool HasItem() => currentItem != null && currentItem.ItemProfileSO != null;

    // ✅ DRAG ĐỂ LẤY ITEM VỀ INVENTORY
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!HasItem()) return;

        CreateDraggingVisual();
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

        if (draggingVisual != null)
        {
            Destroy(draggingVisual);
            draggingVisual = null;
        }

        GameObject target = eventData.pointerEnter;
        bool addedToInventory = false;

        // ✅ Kiểm tra nếu thả vào inventory
        if (target != null && target.GetComponentInParent<NewInventoryUI>() != null)
        {
            InventoryManager.Instance.AddItem(currentItem.ItemProfileSO.itemCode, currentItem.itemCount);
            ObserverManager.Instance.PostEvent(EventID.InventoryChanged);
            addedToInventory = true;
        }

        HauSoundManager.Instance.SpawnSound(Vector3.zero, SoundName.CraftItemDrag);

        if (addedToInventory)
        {
            Clear();
        }
    }

    private void CreateDraggingVisual()
    {
        if (!HasItem()) return;

        draggingVisual = new GameObject("DraggingVisual", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));

        Canvas canvasToUse = targetCanvas ?? GetComponentInParent<Canvas>();
        if (canvasToUse != null)
        {
            draggingVisual.transform.SetParent(canvasToUse.transform, false);
        }

        draggingVisual.transform.SetAsLastSibling();

        RectTransform dragRect = draggingVisual.GetComponent<RectTransform>();
        Vector2 baseSize = rectTransform.sizeDelta;
        if (baseSize.magnitude <= 0) baseSize = new Vector2(64, 64);

        dragRect.sizeDelta = baseSize * dragSizeScale;
        dragRect.anchorMin = Vector2.one * 0.5f;
        dragRect.anchorMax = Vector2.one * 0.5f;
        dragRect.pivot = Vector2.one * 0.5f;
        dragRect.position = Input.mousePosition;

        Image dragImage = draggingVisual.GetComponent<Image>();
        dragImage.sprite = currentItem.ItemProfileSO.itemSprite;
        dragImage.raycastTarget = false;

        CanvasGroup cg = draggingVisual.GetComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
        cg.alpha = 0.8f;

        Canvas dragCanvas = draggingVisual.AddComponent<Canvas>();
        dragCanvas.overrideSorting = true;
        dragCanvas.sortingOrder = 1000;
    }

    // ✅ CLICK ĐỂ LẤY ITEM (ALTERNATIVE)
    public void OnClick()
    {
        if (!HasItem()) return;

        // Thêm vào inventory
        InventoryManager.Instance.AddItem(currentItem.ItemProfileSO.itemCode, currentItem.itemCount);
        ObserverManager.Instance.PostEvent(EventID.InventoryChanged);

        HauSoundManager.Instance.SpawnSound(Vector3.zero, SoundName.CraftItemDrag);

        Clear();

        //Debug.Log($"✅ Đã lấy {currentItem.ItemProfileSO.itemName} x{currentItem.itemCount} về inventory");
    }
}