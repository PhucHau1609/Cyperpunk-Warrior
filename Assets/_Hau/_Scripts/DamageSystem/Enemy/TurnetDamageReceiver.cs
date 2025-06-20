using UnityEngine;

public class TurnetDamageReceiver : DamageReceiver, IDamageResponder, IDamageModifier
{
    [SerializeField] private GameObject explosionPrefab; // Gán prefab hiệu ứng nổ trong Inspector

    void IDamageResponder.OnDead()
    {
        // Tuỳ bạn xử lý thêm gì ở đây
    }

    void IDamageResponder.OnHurt()
    {
        // Tuỳ bạn xử lý bị bắn
    }

    protected override void OnDead()
    {
        base.OnDead();

        // Tạo hiệu ứng nổ tại vị trí hiện tại
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        // Huỷ turret sau khi nổ
        Destroy(gameObject);
    }

    public float ModifyIncomingDamage(float baseDamage)
    {
        if (this.CurrentHP <= 5)
            return baseDamage * 0.5f;
        return baseDamage;
    }
}
