using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

[RequireComponent(typeof(CircleCollider2D))]
public class ItemsPicker : HauMonoBehaviour
{
    [SerializeField] protected CircleCollider2D sphere;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        this.LoadSphereCollider();
    }

    protected virtual void LoadSphereCollider()
    {
        if (this.sphere != null) return;
        this.sphere = GetComponent<CircleCollider2D>();
        this.sphere.radius = 0.6f;
        this.sphere.isTrigger = true;

        Debug.LogWarning(transform.name + ": LoadSphereCollider" + gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.transform.parent == null) return;

        ItemsDropCtrl itemsDropCtrl = other.transform.parent.GetComponent<ItemsDropCtrl>();
        if (itemsDropCtrl == null) return;

        ItemCollectionTracker.Instance.OnItemCollected(itemsDropCtrl.ItemCode);
        itemsDropCtrl.Despawn.DoDespawn();
    }
}
