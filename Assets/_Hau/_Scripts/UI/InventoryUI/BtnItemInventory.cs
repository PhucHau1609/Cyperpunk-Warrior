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

    public virtual void SetItem(ItemInventory itemInventory) //E71 create
    { 
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
        Debug.Log("Item CLick");
    }
}
