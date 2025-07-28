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
        bool wasEmpty = this.itemInventories.Count == 0;

        if (!item.ItemProfileSO.isStackable || itemExist == null)
        {
            item.SetId(Random.Range(0, 999999999));
            this.itemInventories.Add(item);
        }
        else
        {
            itemExist.itemCount += item.itemCount;
        }

        // ? N?u ðây là v?t ph?m ð?u tiên
        if (wasEmpty && this.itemInventories.Count > 0)
        {
            ObserverManager.Instance?.PostEvent(EventID.FirstItemPickedUp, null);
        }
    }


    /*  public virtual void AddItem(ItemInventory item)
      {
          ItemInventory itemExist = this.FindItem(item.ItemProfileSO.itemCode);
          bool wasEmpty = this.itemInventories.Count == 0;


          if (!item.ItemProfileSO.isStackable || itemExist ==  null )
          {
              item.SetId(Random.Range(0, 999999999)); //E71 create
              this.itemInventories.Add(item);
              return;
          }

          itemExist.itemCount += item.itemCount;



      }*/

    public virtual bool RemoveItem(ItemInventory item) //E75 create
    {
        //Debug.Log($"[InventoryCtrl] RemoveItem called - Item: {item.ItemProfileSO.itemCode}, Count: {item.itemCount}");
        //Debug.Log($"[InventoryCtrl] Current inventory items count: {this.itemInventories.Count}");

        ItemInventory itemExist = this.FindItemNotEmpty(item.ItemProfileSO.itemCode);
        if (itemExist == null)
        {
            //Debug.LogWarning($"[InventoryCtrl] Item not found or empty: {item.ItemProfileSO.itemCode}");
            return false;
        }

        //Debug.Log($"[InventoryCtrl] Found item: {itemExist.ItemProfileSO.itemCode}, Current count: {itemExist.itemCount}");

        if (!itemExist.CanDeduct(item.itemCount))
        {
            //Debug.LogWarning($"[InventoryCtrl] Cannot deduct {item.itemCount} from {itemExist.itemCount}");
            return false;
        }

        itemExist.Deduct(item.itemCount);
        //Debug.Log($"[InventoryCtrl] After deduct - Item count: {itemExist.itemCount}");

        if (itemExist.itemCount == 0)
        {
            //Debug.Log($"[InventoryCtrl] Item count is 0, removing from inventory: {itemExist.ItemProfileSO.itemCode}");
            this.itemInventories.Remove(itemExist);
            //Debug.Log($"[InventoryCtrl] After removal - Inventory items count: {this.itemInventories.Count}");
        }

        return true;
    }

    /*  public virtual bool RemoveItem(ItemInventory item) //E75 create
      {
          ItemInventory itemExist = this.FindItemNotEmpty(item.ItemProfileSO.itemCode);
          if (itemExist == null) return false;
          if (!itemExist.CanDeduct(item.itemCount)) return false;
          itemExist.Deduct(item.itemCount);

          if (itemExist.itemCount == 0) this.itemInventories.Remove(itemExist);
          return true;
      }*/

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
