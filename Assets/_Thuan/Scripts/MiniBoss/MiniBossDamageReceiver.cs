using UnityEngine;

public class MiniBossDamageReceiver : DamageReceiver
{
    private IDamageResponder responder;
    public MiniBoss miniBoss;
    protected override void Awake()
    {
        base.Awake();
        responder = GetComponent<IDamageResponder>();
        if (responder == null)
            Debug.LogWarning($"{name} is missing IDamageResponder implementation.");
    }

    protected override void OnHurt()
    {
        miniBoss?.OnHurt();
    }

    protected override void OnDead()
    {
        miniBoss?.OnDead();
    }
}
