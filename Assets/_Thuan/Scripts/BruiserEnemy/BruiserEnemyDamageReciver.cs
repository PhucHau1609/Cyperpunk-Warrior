using System.Collections;
using UnityEngine;

public class BruiserEnemyDamageReceiver : DamageReceiver
{
    private IDamageResponder responder;
    public float damageReductionPercent = 0.5f;

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

    public override int Deduct(int damage)
    {
        if (this.CurrentHP <= this.MaxHP)
        {
            damage = Mathf.RoundToInt(damage * (1 - damageReductionPercent));
        }

        // Trừ máu như bình thường
        if (!this.IsImmotal) this.currentHP -= damage;

        if (this.IsDead()) this.OnDead();
        else this.OnHurt();

        if (this.currentHP < 0) this.currentHP = 0;
        return this.currentHP;
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
