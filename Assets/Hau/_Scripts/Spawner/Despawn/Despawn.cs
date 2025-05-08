using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Despawn<T> : DespawnBase where T : PoolObj // Day 5 - E39, E40 Add Despawn<T>
{
    [SerializeField] protected T parent;  // Day 6 - E40
    [SerializeField] protected Spawner<T> spawner; // Day 6 - E40
    [SerializeField] protected float timeLife = 7f;
    [SerializeField] protected float currentTime = 7f;
    [SerializeField] protected bool isDespawnByTime = true; //E48 create

    protected override void LoadComponents()
    {
        base.LoadComponents();
        this.LoadParent(); // Day 6 - E40
        this.LoadSpawner(); // Day 6 - E41
    }

    protected virtual void FixedUpdate()
    {
        this.DespawnByTime(); //E48 rename
    }

    protected virtual void LoadParent() // Day 6 - E40
    {
        if(this.parent != null) return;
        this.parent = transform.parent.GetComponent<T>();
        Debug.LogWarning(transform.name + ": LoadParent", gameObject);
    }

    protected virtual void LoadSpawner() // Day 6 - E41
    {
        if (this.spawner != null) return;
        this.spawner = GameObject.FindAnyObjectByType<Spawner<T>>();
        Debug.LogWarning(transform.name + ": LoadSpawner", gameObject);
    }
    protected virtual void DespawnByTime()
    {
        if (!this.isDespawnByTime) return;

        this.currentTime -= Time.fixedDeltaTime;
        if (this.currentTime > 0) return;
        this.DoDespawn(); // E43,E44 remake
        this.currentTime = this.timeLife;
    }

    public override void DoDespawn() // E43,E44 create
    {
        this.spawner.Despawn(this.parent); // Day 6 - E40
    }

}
