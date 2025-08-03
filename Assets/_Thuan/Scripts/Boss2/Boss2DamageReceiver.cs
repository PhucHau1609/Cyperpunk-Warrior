using UnityEngine;

public class Boss2DamageReceiver : DamageReceiver
{
    private IDamageResponder responder;
    public float damageReductionPercent = 0.5f;
    private bool shieldActivated = false;
    [SerializeField] private Boss2Controller boss2Controller;
    
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
        boss2Controller.OnHurt();
    }

    protected override void OnDead()
    {
        boss2Controller.OnDead();
    }

    public override int Deduct(int damage)
    {
        // Kích hoạt shield khi xuống dưới 50% máu
        if (this.CurrentHP < this.MaxHP / 2 && !shieldActivated)
        {
            shieldActivated = true;
            boss2Controller.ActivateShield();
        }

        // Nếu shield đang active, Boss2 không nhận damage
        if (boss2Controller.IsShieldActive())
        {
            return this.currentHP; // Không trừ máu
        }

        if (this.CurrentHP < this.MaxHP / 2)
        {
            damage = Mathf.RoundToInt(damage * (1 - damageReductionPercent));
        }

        // Logic damage bình thường khi shield không active
        if (!this.IsImmotal) this.currentHP -= damage;

        if (this.IsDead()) this.OnDead();
        else this.OnHurt();

        if (this.currentHP < 0) this.currentHP = 0;
        return this.currentHP;
    }
    
    // Method để reset máu Boss2 về ban đầu
    public void ResetBossHealth()
    {
        this.currentHP = initialHP;
        this.shieldActivated = false; // Reset shield activation flag
    }
}