using UnityEngine;

public class WallDamageReceiver : DamageReceiver, IDamageResponder
{
    private IDamageResponder responder;

    protected override void Awake()
    {
        base.Awake();
        responder = GetComponent<IDamageResponder>();
        if (responder == null)
            Debug.LogWarning($"{name} is missing IDamageResponder implementation.");
    }

    protected override void OnHurt()
    {
        responder?.OnHurt();
    }

    protected override void OnDead()
    {
        responder?.OnDead();
    }

    void IDamageResponder.OnHurt()
    {
        //
    }

    void IDamageResponder.OnDead()
    {
       //
    }
}
