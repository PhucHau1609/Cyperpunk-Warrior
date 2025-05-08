using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PoolObj : HauMonoBehaviour //Day 6 - E41
{
    [SerializeField] protected DespawnBase despawn;
    public DespawnBase Despawn => despawn;

    public abstract string GetName();

    protected override void LoadComponents()
    {
        base.LoadComponents();
        this.LoadDespawn();
    }

    protected virtual void LoadDespawn()
    {
        if(this.despawn != null) return;
        this.despawn = transform.GetComponentInChildren<DespawnBase>();
        Debug.LogWarning(transform.name + ": LoadDespawn", gameObject);
    }
}
