using UnityEngine;

public class HandDamageReceiver2 : DamageReceiver
{
    private IDamageResponder responder;
    public float damageReductionPercent = 0.5f;

    [SerializeField] private Boss2DamageReceiver boss2DamageReceiver;
    protected override void Awake()
    {
        base.Awake();
        responder = GetComponent<IDamageResponder>();
    }

    protected override void OnHurt()
    {
        responder?.OnHurt();
    }

    protected override void OnDead()
    {
        responder?.OnDead();
    }

    public override int Deduct(int damage)
    {
        if (boss2DamageReceiver != null)
        {
            boss2DamageReceiver.TakeDamage(damage);
            this.OnHurt();
        }
        else
        {

        }
        return this.CurrentHP;
    }
}
