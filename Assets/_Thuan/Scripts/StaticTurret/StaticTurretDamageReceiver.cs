using UnityEngine;

public class StaticTurretDamageReceiver : DamageReceiver
{
    private IDamageResponder responder;
    public StaticTurret staticTurret;
    protected override void Awake()
    {
        base.Awake();
        responder = GetComponent<IDamageResponder>();
        if (responder == null)
            Debug.LogWarning($"{name} is missing IDamageResponder implementation.");
    }

    protected override void OnHurt()
    {
        staticTurret?.OnHurt();
    }

    protected override void OnDead()
    {
        staticTurret?.OnDead();
    }
}
