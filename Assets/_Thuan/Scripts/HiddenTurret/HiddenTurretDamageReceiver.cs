using System.Collections;
using UnityEngine;

public class HiddenTurretDamageReceiver : DamageReceiver
{
    private IDamageResponder responder;
    public HiddenTurret hiddenTurret;
    protected override void Awake()
    {
        base.Awake();
        responder = GetComponent<IDamageResponder>();
    }

    protected override void OnHurt()
    {
        hiddenTurret?.OnHurt();
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
    }
}
