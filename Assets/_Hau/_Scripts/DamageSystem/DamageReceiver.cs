using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DamageReceiver : HauMonoBehaviour, IDamageable // E43,E44 create
{
    [SerializeField] protected int currentHP = 10;
    public int CurrentHP => currentHP;

    [SerializeField] protected int maxHP = 10;
    public int MaxHP => maxHP;

    protected bool isDead = false;
    [SerializeField] protected bool IsImmotal = false; // E45 create

    protected override void OnEnable() //E49,50 create
    {
        this.RebornEnemyFromPool(); //E49,50 create
    }

    public virtual int Deduct(int damage)
    {
        if(!this.IsImmotal) this.currentHP -= damage; // E45 create

        if (this.IsDead()) this.OnDead(); // E45 create
        else this.OnHurt(); // E45 create


        if (this.currentHP < 0) this.currentHP = 0;
        return this.currentHP;
    }

    public virtual bool IsDead()
    {
        return this.currentHP <= 0;
    }

    protected virtual void OnDead() // E45 create
    {
        //For override to do animation.....
    }

    protected virtual void OnHurt() // E45 create
    {
        //For override to do 
    }

    protected virtual void RebornEnemyFromPool() //E49,50 create
    {
        this.currentHP = this.maxHP;
    }

    public void TakeDamage(int damage)
    {
        Deduct(damage);
    }
}


/*  public virtual bool IsDead()
     {
         return this.isDead = this.currentHP <= 0; //currentHP > 0 -> false . and else
     }*/