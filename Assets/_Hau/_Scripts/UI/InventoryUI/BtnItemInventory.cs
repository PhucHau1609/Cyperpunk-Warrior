using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BtnItemInventory : ButtonAbstract, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] protected TextMeshProUGUI txtItemName;
    [SerializeField] protected Text txtItemCount;
    [SerializeField] protected Image itemImage;
    [SerializeField] protected ItemInventory itemInventory;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Vector3 originalPosition;

    [Header("Drag Settings")]
    [SerializeField] private float dragSizeScale = 0.8f; 
    private GameObject draggingVisual;


    protected override void Start()
    {
        base.Start();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (itemInventory == null || itemInventory.ItemProfileSO == null) return;

        originalPosition = rectTransform.position;
        canvasGroup.blocksRaycasts = false;

        // ✅ Tạo visual drag để hiển thị liên tục khi kéo
        CreateDraggingVisual();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggingVisual != null)
        {
            // ✅ Cập nhật vị trí của visual drag theo mouse
            draggingVisual.transform.position = Input.mousePosition;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        // ✅ Destroy visual drag
        if (draggingVisual != null)
        {
            Destroy(draggingVisual);
            draggingVisual = null;
        }

        // Kiểm tra nếu thả vào Delete Zone
        if (DeleteItemZone.IsPointerOverDeleteZone(eventData))
        {
            Debug.Log("Delete item: " + itemInventory.ItemProfileSO.itemCode);
            InventoryManager.Instance.RemoveItem(itemInventory);
            return;
        }

        // ✅ Không cần trả về vị trí ban đầu vì item vẫn ở inventory
        // rectTransform.position = originalPosition; // Removed
    }

    // ✅ Tạo visual drag
    private void CreateDraggingVisual()
    {
        if (itemInventory == null || itemInventory.ItemProfileSO == null) return;

        // Tạo GameObject tạm để drag
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

        draggingVisual.transform.SetAsLastSibling(); // Hiển thị trên cùng

        // Setup RectTransform
        RectTransform dragRect = draggingVisual.GetComponent<RectTransform>();
        dragRect.sizeDelta = itemImage.rectTransform.sizeDelta * dragSizeScale;
        dragRect.position = Input.mousePosition;

        // Setup Image
        Image dragImage = draggingVisual.GetComponent<Image>();
        dragImage.sprite = itemInventory.ItemProfileSO.itemSprite;
        dragImage.raycastTarget = false;

        // Setup CanvasGroup
        CanvasGroup dragCanvasGroup = draggingVisual.GetComponent<CanvasGroup>();
        dragCanvasGroup.blocksRaycasts = false;
        dragCanvasGroup.alpha = 0.8f; // Làm mờ một chút để dễ nhìn
    }

    public ItemInventory ItemInventory => itemInventory;

    private void FixedUpdate()
    {
        this.ItemTextUpdating();
    }

    protected override void LoadComponents()
    {
        base.LoadComponents();
        this.LoadItemCount();
        this.LoadItemImage();
    }

    protected virtual void LoadItemName()
    {
        if (this.txtItemName != null) return;
        this.txtItemName = transform.Find("ItemName").GetComponent<TextMeshProUGUI>();
        Debug.Log(transform.name + ": LoadItemName", gameObject);
    }

    protected virtual void LoadItemCount()
    {
        if (this.txtItemCount != null) return;
        this.txtItemCount = transform.Find("Badget/Image/ItemCount").GetComponent<Text>();
        Debug.Log(transform.name + ": LoadItemCount", gameObject);
    }

    protected virtual void LoadItemImage()
    {
        if (this.itemImage != null) return;
        this.itemImage = transform.Find("ItemImage").GetComponent<Image>();
        Debug.Log(transform.name + ": LoadItemImage", gameObject);
    }

    public virtual void SetItem(ItemInventory itemInventory)
    {
        if (itemInventory.ItemProfileSO == null)
            Debug.LogError("❌ BtnItemInventory được SetItem nhưng không có ItemProfileSO!");
        this.itemInventory = itemInventory;
    }

    private void ItemTextUpdating()
    {
        if (itemInventory == null || itemInventory.ItemProfileSO == null) return;

        this.txtItemCount.text = this.itemInventory.itemCount.ToString();
        this.itemImage.sprite = this.itemInventory.ItemProfileSO.itemSprite;

        if (this.itemInventory.itemCount == 0) Destroy(gameObject);
    }

    protected override void OnClick()
    {
        Debug.Log("Item CLick:" + gameObject.name);
    }
}

/*public class BtnItemInventory : ButtonAbstract, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] protected TextMeshProUGUI txtItemName;
    [SerializeField] protected Text txtItemCount;
    [SerializeField] protected Image itemImage;
    [SerializeField] protected ItemInventory itemInventory;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Vector3 originalPosition;

    protected override void Start()
    {
        base.Start();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPosition = rectTransform.position;
        canvasGroup.blocksRaycasts = false; // Để nhận Drop
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        // Kiểm tra nếu thả vào Delete Zone
        if (DeleteItemZone.IsPointerOverDeleteZone(eventData))
        {
            Debug.Log("Delete item: " + itemInventory.ItemProfileSO.itemCode);
            InventoryManager.Instance.RemoveItem(itemInventory);
        }

        // Trả về vị trí ban đầu
        rectTransform.position = originalPosition;
    }

    public ItemInventory ItemInventory => itemInventory; //E71 create

    private void FixedUpdate()
    {
        this.ItemTextUpdating(); //E73 create
    }
    protected override void LoadComponents()
    {
        base.LoadComponents();
        //this.LoadItemName(); //E73 create
        this.LoadItemCount(); //E73 create
        this.LoadItemImage();
    }

    protected virtual void LoadItemName()
    {
        if (this.txtItemName != null) return;
        this.txtItemName = transform.Find("ItemName").GetComponent<TextMeshProUGUI>();
        Debug.Log(transform.name + ": LoadItemName", gameObject);
    }

    protected virtual void LoadItemCount()
    {
        if (this.txtItemCount != null) return;
        this.txtItemCount = transform.Find("Badget/Image/ItemCount").GetComponent<Text>();
        Debug.Log(transform.name + ": LoadItemCount", gameObject);
    }

    protected virtual void LoadItemImage()
    {
        if (this.itemImage != null) return;
        this.itemImage = transform.Find("ItemImage").GetComponent<Image>();
        Debug.Log(transform.name + ": LoadItemImage", gameObject);
    }

    public virtual void SetItem(ItemInventory itemInventory)
    {
        if (itemInventory.ItemProfileSO == null)
            Debug.LogError("❌ BtnItemInventory được SetItem nhưng không có ItemProfileSO!");

        this.itemInventory = itemInventory;
    }

    private void ItemTextUpdating() //E73 create
    {
        //this.txtItemName.text = this.itemInventory.GetItemName();
        this.txtItemCount.text = this.itemInventory.itemCount.ToString();
        this.itemImage.sprite = this.itemInventory.ItemProfileSO.itemSprite;

        if (this.itemInventory.itemCount == 0) Destroy(gameObject); //E75 create
    }

    protected override void OnClick()
    {
        Debug.Log("Item CLick:" + gameObject.name);
    }
}*/
