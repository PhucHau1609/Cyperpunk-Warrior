/*using System.Collections;
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
*/

using UnityEngine;

public class ItemsDropDespawn : Despawn<ItemsDropCtrl>
{
    protected override void ResetValue()
    {
        base.ResetValue();
        this.isDespawnByTime = false;
    }

    // Giữ hành vi cũ: mặc định CÓ cộng inventory
    public override void DoDespawn()
    {
        DoDespawn(true);
    }

    /// <summary>
    /// Despawn với lựa chọn có/không cộng inventory.
    /// Dùng cho các case đặc biệt như HP-heal (không vào túi).
    /// </summary>
    public void DoDespawn(bool addToInventory)
    {
        var itemsDropCtrl = (ItemsDropCtrl)this.parent;

        if (addToInventory)
        {
            InventoryManager.Instance.AddItem(itemsDropCtrl.ItemCode, itemsDropCtrl.ItemCount);
        }

        base.DoDespawn();
    }
}
