using UnityEngine;

public class Boss2DamageReceiver : DamageReceiver
{
    private IDamageResponder responder;
    public float damageReductionPercent = 0.5f;
    private bool shieldActivated = false;
    [SerializeField] private Boss2Controller boss2Controller;

    protected override void Awake()
    {
        base.Awake();
        responder = GetComponent<IDamageResponder>();
        if (responder == null)
            Debug.LogWarning($"{name} is missing IDamageResponder implementation.");
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
            Debug.Log("[Boss2] Shield Activated! Now only hands can take damage.");
        }

        // Nếu shield đang active, Boss2 không nhận damage
        if (boss2Controller.IsShieldActive())
        {
            Debug.Log("[Boss2] Shield is blocking damage! Attack the hands instead!");
            return this.currentHP; // Không trừ máu
        }

        if (this.CurrentHP < this.MaxHP / 2)
        {
            damage = Mathf.RoundToInt(damage * (1 - damageReductionPercent));
            Debug.Log($"[Turnet] Armor reduced damage to {damage}");
        }

        // Logic damage bình thường khi shield không active
        if (!this.IsImmotal) this.currentHP -= damage;

        if (this.IsDead()) this.OnDead();
        else this.OnHurt();

        if (this.currentHP < 0) this.currentHP = 0;
        return this.currentHP;
    }
}
