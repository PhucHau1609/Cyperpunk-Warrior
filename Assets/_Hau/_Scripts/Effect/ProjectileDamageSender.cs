using System;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public abstract class ProjectileDamageSender : DamageSender // E43,E44 create
{
    [SerializeField] protected EffectCtrl effectCtrl;
    [SerializeField] protected BoxCollider2D _collider;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        this.LoadSphereCollider();
        this.LoadBulletCtrl();
    }

    protected virtual void LoadBulletCtrl()
    {
        if (this.effectCtrl != null) return;
        this.effectCtrl = GetComponentInParent<EffectCtrl>();

        Debug.LogWarning(transform.name + ": LoadBulletCtrl" + gameObject);
    }
    protected virtual void LoadSphereCollider()
    {
        if (this._collider != null) return;
        this._collider = GetComponent<BoxCollider2D>();
        this._collider.isTrigger = true;
        //this._collider.center = new Vector3()

        Debug.LogWarning(transform.name + ": LoadCollider" + gameObject);
    }

    protected override void Sender(DamageReceiver damageRecever, Collider2D collision)
    {
        base.Sender(damageRecever, collision);
        //this.ShowHitEffect(collision);
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