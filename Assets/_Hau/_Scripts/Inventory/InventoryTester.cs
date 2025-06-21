using com.cyborgAssets.inspectorButtonPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryTester : HauMonoBehaviour //E75 create
{
   /* protected override void Start()
    {
        base.Start();
        this.AddTestItems(ItemCode.Gold, 1000);
    }*/

    [ProButton]
    public virtual void AddTestItems(ItemCode itemCode, int count)
    {
        for (int i = 0; i < count; i++)
        {
            InventoryManager.Instance.AddItem(itemCode, 1);
        }
    }

    [ProButton]
    public virtual void RemoveTestItems(ItemCode itemCode, int count)
    {
        for (int i = 0; i < count; i++)
        {
            InventoryManager.Instance.RemoveItem(itemCode, 1);
        }
    }


    [ProButton]
    public virtual void PlaySound()
    {
        HauSoundManager.Instance.SpawnSound(Vector3.zero,SoundName.PickUpItem);
    }
}
