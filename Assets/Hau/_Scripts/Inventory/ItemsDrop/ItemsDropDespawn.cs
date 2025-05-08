using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemsDropDespawn : Despawn<ItemsDropCtrl>
{
    protected override void ResetValue()
    {
        base.ResetValue();
        this.isDespawnByTime = false;
    }
    public override void DoDespawn() 
    {
        ItemsDropCtrl itemsDropCtrl = (ItemsDropCtrl)this.parent;

        InventoryManager.Instance.AddItem(itemsDropCtrl.ItemCode, itemsDropCtrl.ItemCount);

        base.DoDespawn();
    }
}
