using com.cyborgAssets.inspectorButtonPro;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NewInventoryUI : HauSingleton<NewInventoryUI>
{
    [SerializeField] protected Transform showHide;

    [SerializeField] protected BtnItemInventory defaultItemInventoryUI;

    protected List<BtnItemInventory> btnItems = new();

    protected Dictionary<int, BtnItemInventory> itemBtnDict = new();

    protected bool isShowUI = true;
    public bool IsShowUI => isShowUI;

    [SerializeField] protected Vector3 centerPosition = Vector3.zero; // vị trí trung tâm
    [SerializeField] protected Vector3 offsetWhenCraftingOpen = new Vector3(-300f, 0, 0); // vị trí khi crafting mở

    protected override void OnEnable()
    {
        StartCoroutine(WaitForObserverManager());
    }

    private IEnumerator WaitForObserverManager()
    {
        while (ObserverManager.Instance == null)
            yield return null;

        ObserverManager.Instance.AddListener(EventID.InventoryChanged, OnInventoryChanged);
        ObserverManager.Instance.AddListener(EventID.OpenInventory, OnOpenInventory);
    }


    protected override void OnDisable()
    {
        if (!ObserverManager.HasInstance) return;
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

        // 👉 Nếu crafting đang mở, thì tắt luôn
        if (CraftingUI.HasInstance) // để tránh null nếu Crafting chưa được khởi tạo
        {
            CraftingUI.Instance.HideUI();
        }

        // 👉 Tắt tooltip nếu đang hiển thị
        if (ItemTooltipUI.HasInstance)
        {
            ItemTooltipUI.Instance.HideTooltip();
        }
    }

    public virtual void ShowInventoryUI()
    {
        this.isShowUI = true;
        this.showHide.gameObject.SetActive(true);
        this.ItemsUpdating(); 

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
    public virtual void SortInventoryUI()
    {
        // Sắp xếp danh sách btnItems theo số lượng giảm dần
        btnItems = btnItems
            .OrderByDescending(btn => btn.ItemInventory.itemCount)
            .ToList();

        // Cập nhật lại vị trí hiển thị trong hierarchy UI
        for (int i = 0; i < btnItems.Count; i++)
        {
            btnItems[i].transform.SetSiblingIndex(i);
        }
    }

    public void MoveToCenter()
    {
        this.showHide.DOLocalMove(centerPosition, 0.3f);
    }

    public void MoveToSide()
    {
        this.showHide.DOLocalMove(offsetWhenCraftingOpen, 0.3f);
    }
}
