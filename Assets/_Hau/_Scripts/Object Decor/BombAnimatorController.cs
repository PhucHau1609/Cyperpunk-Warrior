using UnityEngine;

public class BombAnimatorController : MonoBehaviour
{
    [SerializeField] protected float damage = 10f;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // Gọi từ AnimationEvent để nổ
    public void TriggerBoom()
    {
        animator.SetTrigger("Explode");
    }

    // Gọi từ cuối animation explode
    public void ExplodeDamage()
    {
        // Tìm Player gần nhất trong bán kính
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 2.5f); // bán kính nổ
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<CharacterController2D>(out var player))
            {
                player.ApplyDamage(damage, transform.position);
                CameraFollow.Instance.ShakeCamera();
            }
        }

        Destroy(gameObject,2f);
    }
}
