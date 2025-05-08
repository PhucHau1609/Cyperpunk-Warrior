using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InventoryCtrl : HauMonoBehaviour
{
    [SerializeField] protected List<ItemInventory> itemInventories = new();
    public List<ItemInventory> ItemInventories => itemInventories; //E71 create

    public abstract InventoryCodeName GetName();

    public virtual void AddItem(ItemInventory item)
    {
        ItemInventory itemExist = this.FindItem(item.ItemProfileSO.itemCode);

        if (!item.ItemProfileSO.isStackable || itemExist ==  null )
        {
            item.SetId(Random.Range(0, 999999999)); //E71 create
            this.itemInventories.Add(item);
            return;
        }

        itemExist.itemCount += item.itemCount;
        
    }

    public virtual bool RemoveItem(ItemInventory item) //E75 create
    {
        ItemInventory itemExist = this.FindItemNotEmpty(item.ItemProfileSO.itemCode);
        if (itemExist == null) return false;
        if (!itemExist.CanDeduct(item.itemCount)) return false;
        itemExist.Deduct(item.itemCount);

        if (itemExist.itemCount == 0) this.itemInventories.Remove(itemExist);
        return true;
    }

    public virtual ItemInventory FindItem(ItemCode itemCode)
    {
        foreach(ItemInventory itemInventory in this.itemInventories)
        {
            if(itemInventory.ItemProfileSO.itemCode == itemCode) return itemInventory;
        }

        return null;
    }

    public virtual ItemInventory FindItemNotEmpty(ItemCode itemCode) //E75 create
    {
        foreach (ItemInventory itemInventory in this.itemInventories)
        {
            if (itemInventory.ItemProfileSO.itemCode != itemCode) continue;
            if(itemInventory.itemCount > 0) return itemInventory;
        }

        return null;
    }
}
