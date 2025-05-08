using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BtnToggleInventory : ButtonAbstract
{
    protected virtual void ToogleInventory()
    {
        InventoryUI.Instance.Toogle();
    }

    protected override void OnClick()
    {
        this.ToogleInventory();
    }
}
