using System.Collections;
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
        if (isDead) return;


        isDead = true; // Gán ở đây, lần đầu chết mới chạy

        base.OnDead();
        responder?.OnDead();
        StartCoroutine(DelayedDrop());
    }


    private IEnumerator DelayedDrop()
    {
        yield return null;

        var drop = GetComponent<ItemDropTable>();
        if (drop != null)
        {
            drop.TryDropItems();
        }
        else
        {
            Debug.Log($"[EnemyDamageReceiver] {name} has NO ItemDropTable");
        }
    }
}
