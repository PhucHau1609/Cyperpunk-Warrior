using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile2HitEffect : ProjectileDamageSender
{
    [SerializeField] float forceKnockBack = 5f;
    protected override string GetHitEffectName()
    {
        //return "Hit_2";
        return _hitName.ToString();
    }

    protected override void Sender(DamageReceiver damageReceiver, Collider2D collision)
    {
        base.Sender(damageReceiver, collision);

        var knockbackable = damageReceiver as IKnockbackable;
        if (knockbackable != null)
        {
            Vector2 direction = (collision.transform.position - transform.position).normalized;
            knockbackable.ApplyKnockback(direction * forceKnockBack);
        }
    }
}
