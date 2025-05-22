using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BtnCloseInventory : ButtonAbstract
{
    protected virtual void CloseInventory()
    {
        NewInventoryUI.Instance.HideInventoryUI();
    }

    protected override void OnClick()
    {
        this.CloseInventory();
    }
}
