using UnityEngine;

public class SlimeEnemyDamageReciver : DamageReceiver
{
    private IDamageResponder responder;
    public SlimeEnemy slimeEnemy;
    protected override void Awake()
    {
        base.Awake();
        responder = GetComponent<IDamageResponder>();
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
