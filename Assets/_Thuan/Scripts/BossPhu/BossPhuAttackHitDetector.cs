using UnityEngine;

public class BossPhuAttackHitDetector : MonoBehaviour
{
    [Header("Attack Detection Settings")]
    public LayerMask playerLayer = -1;
    public int damage = 5;
    
    [Header("Attack Hit Boxes")]
    [SerializeField] private Transform punchHitBox;    // Vị trí hit box cho đấm
    [SerializeField] private Transform slamHitBox;     // Vị trí hit box cho đập
    
    [Header("Hit Box Sizes")]
    [SerializeField] private Vector2 punchHitSize = new Vector2(2f, 1.5f);
    [SerializeField] private Vector2 slamHitSize = new Vector2(3f, 2f);
    
    private BossPhuController bossController;
    private bool canDealDamage = true;
    
    void Start()
    {
        bossController = GetComponent<BossPhuController>();
        
        // Tạo hit boxes nếu chưa có
        CreateHitBoxes();
    }
    
    void CreateHitBoxes()
    {
        if (punchHitBox == null)
        {
            GameObject punchHitObj = new GameObject("PunchHitBox");
            punchHitObj.transform.SetParent(transform);
            punchHitObj.transform.localPosition = new Vector3(1f, -0.5f, 0f); // Trước mặt boss
            punchHitBox = punchHitObj.transform;
        }
        
        if (slamHitBox == null)
        {
            GameObject slamHitObj = new GameObject("SlamHitBox");
            slamHitObj.transform.SetParent(transform);
            slamHitObj.transform.localPosition = new Vector3(1f, -0.5f, 0f); // Phía trước và hơi xuống
            slamHitBox = slamHitObj.transform;
        }
    }
    
    // Animation Event cho đấm - gọi từ animation Attack1
    public void OnPunchHit()
    {
        if (!canDealDamage) return;
        CheckHit(punchHitBox.position, punchHitSize, "Punch");
    }
    
    // Animation Event cho đập - gọi từ animation Attack2  
    public void OnSlamHit()
    {
        if (!canDealDamage) return;
        CheckHit(slamHitBox.position, slamHitSize, "Slam");
    }
    
    private void CheckHit(Vector3 hitPosition, Vector2 hitSize, string attackType)
    {
        // Điều chỉnh vị trí hit box theo hướng của boss
        Vector3 adjustedPosition = hitPosition;
        if (transform.localScale.x < 0) // Boss quay trái
        {
            adjustedPosition.x = transform.position.x - (hitPosition.x - transform.position.x);
        }
        
        // Kiểm tra va chạm với player
        Collider2D[] hitColliders = Physics2D.OverlapBoxAll(adjustedPosition, hitSize, 0f, playerLayer);
        
        foreach (Collider2D hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
            {
                // Tìm component PlayerStats hoặc tương tự để trừ máu
                CharacterController2D playerController = hitCollider.GetComponent<CharacterController2D>();
                if (playerController != null)
                {
                    playerController.ApplyDamage(damage, transform.position);
                    
                    // Hiệu ứng knockback nếu cần
                    ApplyKnockback(hitCollider);
                    
                    // Disable damage để tránh hit nhiều lần trong 1 animation
                    DisableDamageTemporarily();
                    break;
                }
            }
        }
    }
    
    private void ApplyKnockback(Collider2D playerCollider)
    {
        Rigidbody2D playerRb = playerCollider.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            Vector2 knockbackDirection = (playerCollider.transform.position - transform.position).normalized;
            float knockbackForce = 10f;
            playerRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        }
    }
    
    private void DisableDamageTemporarily()
    {
        canDealDamage = false;
        Invoke("EnableDamage", 0f); // Đợi 0.5s trước khi có thể gây damage lại
    }
    
    private void EnableDamage()
    {
        canDealDamage = true;
    }
    
    // Animation Event được gọi khi animation attack kết thúc
    public void OnAttackEnd()
    {
        EnableDamage();
    }
    
    void OnDrawGizmosSelected()
    {
        if (punchHitBox != null)
        {
            Gizmos.color = Color.red;
            Vector3 punchPos = punchHitBox.position;
            if (transform.localScale.x < 0)
            {
                punchPos.x = transform.position.x - (punchHitBox.position.x - transform.position.x);
            }
            Gizmos.DrawWireCube(punchPos, punchHitSize);
        }
        
        if (slamHitBox != null)
        {
            Gizmos.color = Color.blue;
            Vector3 slamPos = slamHitBox.position;
            if (transform.localScale.x < 0)
            {
                slamPos.x = transform.position.x - (slamHitBox.position.x - transform.position.x);
            }
            Gizmos.DrawWireCube(slamPos, slamHitSize);
        }
    }
}