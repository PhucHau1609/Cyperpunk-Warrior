using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using com.cyborgAssets.inspectorButtonPro;

public class InventoryManager : HauSingleton<InventoryManager>
{
    [SerializeField] protected List<InventoryCtrl> inventoryCtrlList;

    [SerializeField] protected List<ItemProfileSO> itemProfileSO;
    protected override void LoadComponents()
    {
        base.LoadComponents();
        this.LoadInventoryCtrl();
        this.LoadItemProfiles(); //E71 create
    }

    public virtual void AddItem(ItemInventory itemInventory)
    {
        InventoryCodeName invCodeName = itemInventory.ItemProfileSO.invCodeName;
        InventoryCtrl inventoryCtrl = InventoryManager.Instance.GetInventoryByName(invCodeName);
        inventoryCtrl.AddItem(itemInventory);
    }

    public virtual void AddItem(ItemCode itemCode, int itemCount)
    {
        ItemProfileSO itemProfile = InventoryManager.Instance.GetProfileByCode(itemCode);
        ItemInventory item = new(itemProfile, itemCount);
        this.AddItem(item);
    }

    public virtual void RemoveItem(ItemCode itemCode, int itemCount)
    {
        ItemProfileSO itemProfile = InventoryManager.Instance.GetProfileByCode(itemCode);
        ItemInventory item = new(itemProfile, itemCount);
        this.RemoveItem(item);
    }

    public virtual void RemoveItem(ItemInventory itemInventory)
    {
        //Debug.Log($"[InventoryManager] RemoveItem called - Item: {itemInventory.ItemProfileSO.itemCode}, Count: {itemInventory.itemCount}");

        InventoryCodeName invCodeName = itemInventory.ItemProfileSO.invCodeName;
        InventoryCtrl inventoryCtrl = InventoryManager.Instance.GetInventoryByName(invCodeName);

        if (inventoryCtrl == null)
        {
            //Debug.LogError($"[InventoryManager] Inventory not found: {invCodeName}");
            return;
        }

        //Debug.Log($"[InventoryManager] Found inventory: {invCodeName}, Items count before: {inventoryCtrl.ItemInventories.Count}");

        bool removed = inventoryCtrl.RemoveItem(itemInventory);

        //Debug.Log($"[InventoryManager] Remove result: {removed}, Items count after: {inventoryCtrl.ItemInventories.Count}");

        if (removed)
        {
            ObserverManager.Instance?.PostEvent(EventID.InventoryChanged, null);
        }
    }

    protected virtual void LoadInventoryCtrl()
    {
        if (this.inventoryCtrlList.Count > 0) return;

        foreach(Transform child in transform)
        {
            InventoryCtrl inventoryCtrl = child.GetComponent<InventoryCtrl>();
            if(inventoryCtrl == null) continue;
            this.inventoryCtrlList.Add(inventoryCtrl);
        }

        Debug.LogWarning(transform.name + ": LoadInventoryCtrl", gameObject);
    }

    protected virtual void LoadItemProfiles() ////E71 create
    {
        if (this.itemProfileSO.Count > 0) return;
        ItemProfileSO[] itemProfileSOs = Resources.LoadAll<ItemProfileSO>("/");
        this.itemProfileSO = new List<ItemProfileSO>(itemProfileSOs);
        Debug.Log(transform.name + ": LoadItemProfiles", gameObject);
    }

    public virtual InventoryCtrl GetInventoryByName(InventoryCodeName inventoryCodeName)
    {
        foreach(InventoryCtrl inventoryCtrl in this.inventoryCtrlList)
        {
            if(inventoryCtrl.GetName()  == inventoryCodeName) return inventoryCtrl;
        }

        return null;
    }

    public virtual ItemProfileSO GetProfileByCode(ItemCode itemCode)
    {
        foreach (ItemProfileSO itemProfile in this.itemProfileSO)
        {
            if (itemProfile.itemCode == itemCode) return itemProfile;
        }

        return null;
    }


    public virtual InventoryCtrl MoneyInventory() //E67 create
    {
        return this.GetInventoryByName(InventoryCodeName.Currency);
    }

    public virtual InventoryCtrl ItemInventory() //E67 create
    {
        return this.GetInventoryByName(InventoryCodeName.Items);
    }

    // Helper kiểm tra trong inventory có item và count > 0 hay không
    public bool HasItemInInventory(ItemCode itemCode)
    {
        InventoryCtrl itemInv = ItemInventory();
        if (itemInv == null) return false;
        ItemInventory found = itemInv.FindItem(itemCode);
        if (found == null) return false;
        return found.itemCount > 0;
    }


}


/* public virtual void RemoveItem(ItemInventory itemInventory)
  {
      InventoryCodeName invCodeName = itemInventory.ItemProfileSO.invCodeName;
      InventoryCtrl inventoryCtrl = InventoryManager.Instance.GetInventoryByName(invCodeName);
      inventoryCtrl.RemoveItem(itemInventory);

      ObserverManager.Instance?.PostEvent(EventID.InventoryChanged, null); // <- thêm dòng này

  }*/
