using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewInventoryUI : HauSingleton<InventoryUI>
{
    [SerializeField] protected Transform showHide;

    [SerializeField] protected BtnItemInventory defaultItemInventoryUI;

    protected List<BtnItemInventory> btnItems = new();

    protected Dictionary<int, BtnItemInventory> itemBtnDict = new();

    protected bool isShowUI = true;
    public bool IsShowUI => isShowUI;

  /*  private void FixedUpdate()
    {
        this.ItemsUpdating(); //E71 create
    }*/

    protected override void OnEnable()
    {
        ObserverManager.Instance.AddListener(EventID.InventoryChanged, OnInventoryChanged);
        ObserverManager.Instance.AddListener(EventID.OpenInventory, OnOpenInventory);
    }

    protected override void OnDisable()
    {
        ObserverManager.Instance.RemoveListener(EventID.InventoryChanged, OnInventoryChanged);
        ObserverManager.Instance.RemoveListener(EventID.OpenInventory, OnOpenInventory);
    }



    protected override void Start()
    {
        base.Start();
        this.HideInventoryUI();
        this.HideDefaultItemInventory();
    }

    protected override void LoadComponents()
    {
        base.LoadComponents();
        this.LoadBtnItemInventory();
        this.LoadShowHide();
    }

    protected virtual void LoadShowHide() //E76 Create
    {
        if (this.showHide != null) return;
        this.showHide = transform.Find("ShowHide");
        Debug.LogWarning(transform.name + ": LoadShowHide: " + gameObject);
    }

    protected virtual void LoadBtnItemInventory()
    {
        if (this.defaultItemInventoryUI != null) return;
        this.defaultItemInventoryUI = GetComponentInChildren<BtnItemInventory>();
        Debug.LogWarning(transform.name + ": LoadBtnItemInventory: " + gameObject);
    }

    protected virtual void HideDefaultItemInventory()
    {
        this.defaultItemInventoryUI.gameObject.SetActive(false);
    }

    public virtual void HideInventoryUI()
    {
        this.showHide.gameObject.SetActive(false);
        this.isShowUI = false;
    }

    public virtual void ShowInventoryUI()
    {
        this.isShowUI = true;
        this.showHide.gameObject.SetActive(true);
        this.ItemsUpdating(); // 🔥 Thêm dòng này

    }

    protected virtual void OnOpenInventory(object param)
    {
        this.Toogle();
    }

    protected virtual void OnInventoryChanged(object param)
    {
        Debug.Log("Inventory changed, updating UI...");
        this.ItemsUpdating();
    }


    public virtual void Toogle()
    {
        if (this.isShowUI) this.HideInventoryUI();
        else this.ShowInventoryUI();
    }

    protected virtual void ItemsUpdating()
    {
        InventoryCtrl itemInvCtrl = InventoryManager.Instance.ItemInventory();
        foreach (ItemInventory itemInventory in itemInvCtrl.ItemInventories)
        {
            int itemId = itemInventory.ItemId;
            if (!itemBtnDict.ContainsKey(itemId))
            {
                BtnItemInventory newBtnItem = this.CreateItemButton(itemInventory);
                itemBtnDict.Add(itemId, newBtnItem);
                btnItems.Add(newBtnItem);
            }
        }
    }

    protected virtual BtnItemInventory CreateItemButton(ItemInventory itemInventory)
    {
        BtnItemInventory newBtnItem = Instantiate(this.defaultItemInventoryUI);
        newBtnItem.transform.SetParent(this.defaultItemInventoryUI.transform.parent);
        newBtnItem.SetItem(itemInventory);
        newBtnItem.transform.localScale = Vector3.one;
        newBtnItem.gameObject.SetActive(true);
        newBtnItem.name = itemInventory.GetItemName() + "_" + itemInventory.ItemId;
        return newBtnItem;
    }


    protected virtual BtnItemInventory GetExistItem(ItemInventory itemInventory) //E71 create
    {
        foreach (BtnItemInventory itemInvUI in this.btnItems)
        {
            if(itemInvUI.ItemInventory.ItemId == itemInventory.ItemId) return itemInvUI;
        }
        return null;
    }
}

/* protected virtual void ItemsUpdating() //E71 create
    {
        if(!this.isShowUI) return;

        InventoryCtrl itemInvCtrl = InventoryManager.Instance.ItemInventory();
        foreach (ItemInventory itemInventory in itemInvCtrl.ItemInventories)
        {
            BtnItemInventory newBtnItem = this.GetExistItem(itemInventory);
            if (newBtnItem == null)
            {
                newBtnItem = Instantiate(this.defaultItemInventoryUI);
                newBtnItem.transform.SetParent(this.defaultItemInventoryUI.transform.parent);
                newBtnItem.SetItem(itemInventory);
                newBtnItem.transform.localScale = new Vector3(1, 1, 1);
                newBtnItem.gameObject.SetActive(true);
                newBtnItem.name = itemInventory.GetItemName() + "_" + itemInventory.ItemId;
                this.btnItems.Add(newBtnItem);
            }
        }
    }*/

/*

private void LateUpdate() //E76 Create
  {
      this.HotkeyToogleInventory();
  }
  protected virtual void HotkeyToogleInventory() //E76 Create
  {
      if (InputHotKey.Instance.IsToogleInventoryUI) this.Toogle();
  }
 */