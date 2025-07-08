using UnityEngine;

public class Boss2HandHitBox : MonoBehaviour
{
    [Header("Hit Box Settings")]
    public float damage = 10f;
    public float knockbackForce = 15f;
    
    private Boss2HandController handController;
    private Collider2D hitBoxCollider;
    private bool canDamage = false;

    void Awake()
    {
        handController = GetComponentInParent<Boss2HandController>();
        hitBoxCollider = GetComponent<Collider2D>();
        
        if (hitBoxCollider == null)
        {
            Debug.LogError($"{gameObject.name}: HitBox cần có Collider2D!");
        }
        
        // Đảm bảo HitBox bắt đầu ở trạng thái tắt
        DisableHitBox();
    }

    public void EnableHitBox()
    {
        canDamage = true;
        if (hitBoxCollider != null)
        {
            hitBoxCollider.enabled = true;
        }
        Debug.Log($"{gameObject.name}: HitBox enabled");
    }

    public void DisableHitBox()
    {
        canDamage = false;
        if (hitBoxCollider != null)
        {
            hitBoxCollider.enabled = false;
        }
        Debug.Log($"{gameObject.name}: HitBox disabled");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!canDamage) return;

        if (other.CompareTag("Player"))
        {
            // Tìm Player script có hàm ApplyDamage
            var playerScript = other.GetComponent<MonoBehaviour>();
            if (playerScript != null)
            {
                // Gọi hàm ApplyDamage bằng reflection hoặc interface
                var applyDamageMethod = playerScript.GetType().GetMethod("ApplyDamage", 
                    new System.Type[] { typeof(float), typeof(Vector3) });
                
                if (applyDamageMethod != null)
                {
                    applyDamageMethod.Invoke(playerScript, new object[] { damage, transform.position });
                    Debug.Log($"{gameObject.name}: Dealt {damage} damage to Player!");
                    
                    // Tắt HitBox sau khi đánh trúng để tránh damage liên tục
                    DisableHitBox();
                }
                else
                {
                    Debug.LogWarning($"{gameObject.name}: Player không có hàm ApplyDamage!");
                }
            }
        }
    }
}