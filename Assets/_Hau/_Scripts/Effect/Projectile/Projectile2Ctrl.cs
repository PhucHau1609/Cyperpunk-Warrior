using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile2Ctrl : EffectFlyAbstract
{
    [SerializeField] protected DamageSender damageSender;

    public override string GetName()
    {
        //return "Projectile_2";
        return ProjectileName.Projectile_2.ToString();
    }

    protected override void LoadComponents()
    {
        base.LoadComponents();
        this.LoadDamageSender();
    }

    protected virtual void LoadDamageSender()
    {
        if (this.damageSender != null) return;
        this.damageSender = GetComponentInChildren<DamageSender>();
        this.damageSender.SetDamage(4);
        Debug.Log(transform.name + ": LoadDamageSender", gameObject);
    }
}
