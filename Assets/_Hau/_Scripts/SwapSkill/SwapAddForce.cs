using UnityEngine;

public class SwapAddForce : DamageReceiver, IKnockbackable
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float knockbackResetDelay = 0.05f;

    protected override void Awake()
    {
        base.Awake();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
    }

    public void ApplyKnockback(Vector2 force)
    {
        if (rb != null)
        {
            rb.linearVelocity = force; // Nhích nhẹ
            Invoke(nameof(ResetVelocity), knockbackResetDelay); // Dừng lại sau vài frame
        }
    }

    private void ResetVelocity()
    {
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }
}
