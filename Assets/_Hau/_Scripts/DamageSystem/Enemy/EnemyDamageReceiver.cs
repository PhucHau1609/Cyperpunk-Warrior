using System.Collections;
using UnityEngine;

public class EnemyDamageReceiver : DamageReceiver
{
    private IDamageResponder responder;

    // Event để thông báo khi enemy chết
    public System.Action<GameObject> OnDeathEvent;

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
        if (isDead)
        {
            return;
        }

        isDead = true; // Gán ở đây, lần đầu chết mới chạy

        base.OnDead();
        responder?.OnDead();

        // Thông báo cho EnemyManager trước khi chạy responder
        OnDeathEvent?.Invoke(gameObject);

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
