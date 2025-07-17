using UnityEngine;

public class HiddenTurretDamageReceiver : DamageReceiver
{
    private IDamageResponder responder;
    public HiddenTurret hiddenTurret;
    protected override void Awake()
    {
        base.Awake();
        responder = GetComponent<IDamageResponder>();
        if (responder == null)
            Debug.LogWarning($"{name} is missing IDamageResponder implementation.");
    }

    protected override void OnHurt()
    {
        hiddenTurret?.OnHurt();
    }

    protected override void OnDead()
    {
        hiddenTurret?.OnDead();
    }
}
