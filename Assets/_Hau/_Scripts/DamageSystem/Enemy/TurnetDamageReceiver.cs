using UnityEngine;

public class TurnetDamageReceiver : DamageReceiver, IDamageResponder, IDamageModifier
{
    void IDamageResponder.OnDead()
    {
        //throw new System.NotImplementedException();
    }

    void IDamageResponder.OnHurt()
    {
        //throw new System.NotImplementedException();
    }

    protected override void OnDead()
    {
        base.OnDead();
    }

    public float ModifyIncomingDamage(float baseDamage)
    {

        if (this.CurrentHP <= 5)
            return baseDamage * 0.5f;
        return baseDamage;

    }
}


