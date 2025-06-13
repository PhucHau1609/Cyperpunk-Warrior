using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectSpawnerCtrl : HauSingleton<EffectSpawnerCtrl>
{
    [SerializeField] protected EffectSpawner effectSpawner;
    public EffectSpawner EffectSpawner => effectSpawner;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        this.LoadEffectSpawner();
    }

    protected virtual void LoadEffectSpawner()
    {
        if (this.effectSpawner != null) return;
        this.effectSpawner = GetComponent<EffectSpawner>();
        Debug.LogWarning(transform.name + ": LoadEffectSpawner: " + gameObject);

    }
}
