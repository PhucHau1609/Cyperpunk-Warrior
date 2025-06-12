using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EffectFlyAbstract : EffectCtrl
{
    [SerializeField] protected EffectFlyToTarget effectFly;
    public EffectFlyToTarget EffectFlyToTarget => effectFly;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        this.LoadEffectFlyToTarget();
    }

    protected virtual void LoadEffectFlyToTarget()
    {
        if (this.effectFly != null) return;
        this.effectFly = GetComponentInChildren<EffectFlyToTarget>();

        Debug.LogWarning(transform.name + ": LoadEffectFlyToTarget: " + gameObject);
    }
}
