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
        if (responder == null)
            Debug.LogWarning($"{name} is missing IDamageResponder implementation.");
    }

    protected override void OnHurt()
    {
        responder?.OnHurt();
    }

    protected override void OnDead()
    {
        if (isDead)
        {
            Debug.Log($"[EnemyDamageReceiver] {name} OnDead() called but already dead.");
            return;
        }

        isDead = true; // Gán ở đây, lần đầu chết mới chạy
        Debug.Log($"[EnemyDamageReceiver] {name} OnDead() BEGIN");

        base.OnDead();
        responder?.OnDead();

        // Thông báo cho EnemyManager trước khi chạy responder
        OnDeathEvent?.Invoke(gameObject);

        StartCoroutine(DelayedDrop());
    }


    private IEnumerator DelayedDrop()
    {
        yield return null;
        Debug.Log($"[EnemyDamageReceiver] {name} Dropping item now");

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
