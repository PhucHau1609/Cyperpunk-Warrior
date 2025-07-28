using UnityEngine;

public class MiniBossDamageReceiver : DamageReceiver
{
    private IDamageResponder responder;
    public MiniBoss miniBoss;
    
    // Lưu HP ban đầu để reset
    private int initialHP;
    
    protected override void Awake()
    {
        base.Awake();
        responder = GetComponent<IDamageResponder>();
        if (responder == null)
            Debug.LogWarning($"{name} is missing IDamageResponder implementation.");
            
        // Lưu HP ban đầu
        initialHP = maxHP;
    }

    protected override void OnHurt()
    {
        miniBoss?.OnHurt();
    }

    protected override void OnDead()
    {
        miniBoss?.OnDead();
    }
    
    // Method để reset máu MiniBoss về ban đầu
    public void ResetBossHealth()
    {
        this.currentHP = initialHP;
        Debug.Log($"MiniBoss {gameObject.name} health reset to {currentHP}/{maxHP}");
    }
}