using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BtnCloseInventory : ButtonAbstract
{
    protected virtual void CloseInventory()
    {
        InventoryUI.Instance.HideInventoryUI();
    }

    protected override void OnClick()
    {
        this.CloseInventory();
    }
}
