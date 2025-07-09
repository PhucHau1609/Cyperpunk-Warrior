using UnityEngine;

public class HandDamageReceiver : DamageReceiver
{
    private IDamageResponder responder;
    [SerializeField] private Boss2HandController handController;
    [SerializeField] private Boss2Controller boss2Controller; // Thêm reference này

    protected override void Awake()
    {
        base.Awake();
        responder = GetComponent<IDamageResponder>();
        handController = GetComponent<Boss2HandController>();
        
        // Tự động tìm Boss2Controller nếu chưa gán
        if (boss2Controller == null)
        {
            boss2Controller = FindFirstObjectByType<Boss2Controller>();
        }
        
        if (responder == null)
            Debug.LogWarning($"{name} is missing IDamageResponder implementation.");
    }

    protected override void OnHurt()
    {
        if (handController != null)
        {
            handController.OnDamageReceived();
        }
    }

    protected override void OnDead()
    {
        if (handController != null)
        {
            handController.OnDamageReceived();
        }
        handController.OnDead();
    }

    public override int Deduct(int damage)
    {
        // Nếu Boss2 trên 50% máu hoặc shield chưa active
        if (boss2Controller == null || !boss2Controller.IsShieldActive())
        {
            Debug.Log("[HandDamageReceiver] Shield not active, damage goes to Boss2!");
            // Chuyển damage cho Boss2
            if (boss2Controller != null)
            {
                boss2Controller.TakeDamage(damage);
            }
            return this.currentHP; // Cánh tay không nhận damage
        }

        // Nếu shield đang active (Boss2 dưới 50% máu), cánh tay mới nhận damage
        Debug.Log($"[HandDamageReceiver] Shield active, hand takes {damage} damage!");
        
        if (!this.IsImmotal) this.currentHP -= damage;

        if (this.IsDead()) this.OnDead();
        else this.OnHurt();

        if (this.currentHP < 0) this.currentHP = 0;
        return this.currentHP;
    }
}