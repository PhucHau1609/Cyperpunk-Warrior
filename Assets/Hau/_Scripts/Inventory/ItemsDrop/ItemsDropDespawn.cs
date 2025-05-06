using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemsDropDespawn : Despawn<ItemsDropCtrl>
{
    public override void DoDespawn() 
    {
        ItemsDropCtrl itemsDropCtrl = (ItemsDropCtrl)this.parent;

        InventoryManager.Instance.AddItem(itemsDropCtrl.ItemCode, itemsDropCtrl.ItemCount);

        base.DoDespawn();
    }
}
