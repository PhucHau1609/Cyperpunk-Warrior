using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ItemsDropCtrl : PoolObj
{
    [SerializeField] protected Rigidbody2D rb;
    public Rigidbody2D Rigidbody => rb;

    [SerializeField] protected ItemCode itemCode;
    public ItemCode ItemCode => itemCode;

    [SerializeField] protected int itemCount;

    public int ItemCount => itemCount;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        this.LoadRigibody();
    }
    public override string GetName()
    {
        return "ItemDrop";
    }

    public virtual void SetValueItemDrop(ItemCode itemCode, int itemCount)
    {
        this.itemCode = itemCode;
        this.itemCount = itemCount;
    }

    protected virtual void LoadRigibody()
    {
        if (this.rb != null) return;
        this.rb = GetComponent<Rigidbody2D>();

        Debug.LogWarning(transform.name + ": LoadRigibody" + gameObject);
    }
}
