using UnityEngine;

public class SlimeEnemyDamageReciver : DamageReceiver
{
    private IDamageResponder responder;
    public SlimeEnemy slimeEnemy;
    protected override void Awake()
    {
        base.Awake();
        responder = GetComponent<IDamageResponder>();
        if (responder == null)
            Debug.LogWarning($"{name} is missing IDamageResponder implementation.");
    }

    protected override void OnHurt()
    {
        slimeEnemy?.OnHurt();
    }

    protected override void OnDead()
    {
        slimeEnemy?.OnDead();
    }
}
