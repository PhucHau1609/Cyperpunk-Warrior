using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile4Ctrl : EffectFlyAbstract
{
    [SerializeField] protected DamageSender damageSender;

    public override string GetName()
    {
        //return "Projectile_4"; //turn into enum this one
        return ProjectileName.Projectile_4.ToString();
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
        this.damageSender.SetDamage(7);
        Debug.Log(transform.name + ": LoadDamageSender", gameObject);
    }
}
