using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class DamageSender : HauMonoBehaviour // E43,E44 create
{
    [SerializeField] protected int damage = 1;
    [SerializeField] protected Rigidbody2D _rigidbody;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        this.LoadRigibody();
    }
    protected virtual void LoadRigibody()
    {
        if (this._rigidbody != null) return;
        this._rigidbody = GetComponent<Rigidbody2D>();
        //this._rigidbody.useGravity = false;

        Debug.LogWarning(transform.name + ": LoadRigibody" + gameObject);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        DamageReceiver damageReceiver = collision.GetComponent<DamageReceiver>();
        if (damageReceiver == null) return;
        this.Sender(damageReceiver, collision);
        //Debug.Log("DamageSender to: " + collision.name);
    }

    protected virtual void Sender(DamageReceiver damageRecever, Collider2D collision)
    {
        damageRecever.Deduct(this.damage);
    }

    public virtual void SetDamage(int newDamage)
    {
        this.damage = newDamage;
    }
}
