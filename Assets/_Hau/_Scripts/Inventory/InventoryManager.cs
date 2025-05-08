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
        InventoryCodeName invCodeName = itemInventory.ItemProfileSO.invCodeName;
        InventoryCtrl inventoryCtrl = InventoryManager.Instance.GetInventoryByName(invCodeName);
        inventoryCtrl.RemoveItem(itemInventory);
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

}
