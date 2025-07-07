using System;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public abstract class EffectDamageSender : DamageSender // E43,E44 create
{
    [SerializeField] protected EffectCtrl effectCtrl;
    [SerializeField] protected CircleCollider2D _collider;
    [SerializeField] protected HitName _hitName;
    protected override void LoadComponents()
    {
        base.LoadComponents();
        this.LoadCircleCollider();
        this.LoadEffectCtrl();
    }

    protected virtual void LoadEffectCtrl()
    {
        if (this.effectCtrl != null) return;
        this.effectCtrl = GetComponentInParent<EffectCtrl>();
        if(this.effectCtrl == null ) 
        {
            Debug.Log("Not Found Effect Ctrl!");
        }
        Debug.LogWarning(transform.name + ": LoadEffectCtrl" + gameObject);
    }
    protected virtual void LoadCircleCollider()
    {
        if (this._collider != null) return;
        this._collider = GetComponent<CircleCollider2D>();
        this._collider.radius = 0.2f;
        this._collider.isTrigger = true;

        Debug.LogWarning(transform.name + ": LoadCollider" + gameObject);
    }

    protected override void Sender(DamageReceiver damageRecever, Collider2D collision)
    {
        base.Sender(damageRecever, collision);
        this.ShowHitEffect(collision);
        this.effectCtrl.Despawn.DoDespawn();
    }

    private void ShowHitEffect(Collider2D collision)
    {
        Vector3 hitPos = collision.ClosestPoint(transform.position);
        EffectCtrl prefab = EffectSpawnerCtrl.Instance.EffectSpawner.PoolPrefabs.GetPrefabByName(this.GetHitEffectName());
        EffectCtrl newHitEffect = EffectSpawnerCtrl.Instance.EffectSpawner.Spawn(prefab, hitPos);
        newHitEffect.gameObject.SetActive(true);
    }

    protected abstract string GetHitEffectName();
}