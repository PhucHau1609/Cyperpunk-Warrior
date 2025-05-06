using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BtnItemInventory : ButtonAbstract
{
    [SerializeField] protected TextMeshProUGUI txtItemName;
    [SerializeField] protected TextMeshProUGUI txtItemCount;

    [SerializeField] protected ItemInventory itemInventory;

    public ItemInventory ItemInventory => itemInventory; //E71 create

    private void FixedUpdate()
    {
        this.ItemTextUpdating(); //E73 create
    }
    protected override void LoadComponents()
    {
        base.LoadComponents();
        this.LoadItemName(); //E73 create
        this.LoadItemCount(); //E73 create
    }

    protected virtual void LoadItemName()
    {
        if (this.txtItemName != null) return;
        this.txtItemName = transform.Find("ItemName").GetComponent<TextMeshProUGUI>();
        Debug.Log(transform.name + ": LoadItemName", gameObject);
    }

    protected virtual void LoadItemCount()
    {
        if (this.txtItemCount != null) return;
        this.txtItemCount = transform.Find("ItemCount").GetComponent<TextMeshProUGUI>();
        Debug.Log(transform.name + ": LoadItemCount", gameObject);
    }

    public virtual void SetItem(ItemInventory itemInventory) //E71 create
    { 
        this.itemInventory = itemInventory;
    }
    private void ItemTextUpdating() //E73 create
    {
        this.txtItemName.text = this.itemInventory.GetItemName();
        this.txtItemCount.text = this.itemInventory.itemCount.ToString();

        if (this.itemInventory.itemCount == 0) Destroy(gameObject); //E75 create
    }

    protected override void OnClick()
    {
        Debug.Log("Item CLick");
    }
}
