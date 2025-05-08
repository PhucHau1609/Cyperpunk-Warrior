using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemInventory 
{
    protected int itemId;

    public int ItemId => itemId;

    protected ItemProfileSO itemProfileSO;

    public ItemProfileSO ItemProfileSO => itemProfileSO;


    [SerializeField] protected string itemName;
    public int itemCount;

    public ItemInventory(ItemProfileSO itemProfile, int itemCount)
    {
        this.itemProfileSO = itemProfile;
        this.itemCount = itemCount;
        this.itemName = this.itemProfileSO.itemName;
    }

    public virtual void SetId(int itemId)
    {
        this.itemId = itemId;
    }

    public virtual void SetName(string name)
    {
        this.itemName = name;
    }

    public virtual string GetItemName()
    {
        if (this.itemName == null || this.itemName == "") return this.ItemProfileSO.itemName;
        return this.itemName;
    }


    public virtual bool Deduct(int number)
    {
        if(!this.CanDeduct(number)) return false;

        this.itemCount -= number;

        return true;
    }

    public virtual bool CanDeduct(int number)
    {
        return this.itemCount >= number;
    }
}
