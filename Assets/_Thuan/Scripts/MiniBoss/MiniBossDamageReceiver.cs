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
    }
}