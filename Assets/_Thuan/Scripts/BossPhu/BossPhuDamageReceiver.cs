using UnityEngine;

public class BossPhuDamageReceiver : DamageReceiver
{
    private IDamageResponder responder;
    public float damageReductionPercent = 0.5f;
    
    // Lưu HP ban đầu để reset
    private int initialHP;

    protected override void Awake()
    {
        base.Awake();
        responder = GetComponent<IDamageResponder>();
            
        // Lưu HP ban đầu
        initialHP = maxHP;
    }

    protected override void OnHurt()
    {
        responder?.OnHurt();
    }

    protected override void OnDead()
    {
        responder?.OnDead();
    }

    public override int Deduct(int damage)
    {
        if (this.CurrentHP < this.MaxHP / 2)
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
    
    // Method để reset máu boss về ban đầu
    public void ResetBossHealth()
    {
        this.currentHP = initialHP;
    }
}