using UnityEngine;

public class WeaponShooter : WeaponAbstract
{
    [Header("Shoot Settings")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private BulletGunName bulletName;
    [SerializeField] private int bulletDamage = 1;


    [Header("Multi-Shot Settings")]
    public bool enableMultiShot = false;
    [SerializeField, Range(1, 10)] private int bulletCount = 3;
    [SerializeField, Range(0, 90)] private float maxSpreadAngle = 15f;

    [Header("Particle")]
    [SerializeField] private ParticleSystem shellParticle;
    [SerializeField] private ParticleSystem flameParticle;

    private void Update()
    {
        if (!this.gameObject.activeInHierarchy)
        {
            Debug.Log("Weapon is active false!");
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        if (firePoint == null)
        {
            Debug.LogWarning("Chưa gán FirePoint.");
            return;
        }

        Vector3 baseDir = firePoint.TransformDirection(Vector3.right).normalized;

        // Nếu chưa bật Multi Shot thì chỉ bắn 1 viên
        if (!enableMultiShot || bulletCount <= 1)
        {
            FireSingleBullet(baseDir);
        }
        else
        {
            float angleStep = (bulletCount > 1) ? (maxSpreadAngle * 2) / (bulletCount - 1) : 0f;
            float startAngle = -maxSpreadAngle;

            for (int i = 0; i < bulletCount; i++)
            {
                float currentAngle = startAngle + angleStep * i;
                Vector3 rotatedDir = Quaternion.Euler(0, 0, currentAngle) * baseDir;
                FireSingleBullet(rotatedDir);
            }
        }

        this.SpawnSound(this.transform.position);

        if (shellParticle != null) shellParticle.Play();
        if (flameParticle != null) flameParticle.Play();
    }

    private void FireSingleBullet(Vector3 direction)
    {
        EffectCtrl bulletBase = EffectSpawnerCtrl.Instance.EffectSpawner.PoolPrefabs.GetPrefabByName(bulletName.ToString());
        EffectCtrl spawnedBullet = EffectSpawnerCtrl.Instance.EffectSpawner.Spawn(bulletBase, firePoint.position);

        if (spawnedBullet == null)
        {
            Debug.LogWarning("Không tìm thấy hoặc spawn được viên đạn.");
            return;
        }

        spawnedBullet.gameObject.SetActive(true);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        spawnedBullet.transform.rotation = Quaternion.Euler(0, 0, angle);

        if (spawnedBullet is EffectFlyAbstract effectFly)
        {
            if (effectFly.EffectFlyToTarget != null)
            {
                SpriteRenderer spriteRenderer = spawnedBullet.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.flipX = direction.x < 0;
                }

                Vector3 localScale = spawnedBullet.transform.localScale;
                localScale.x = Mathf.Abs(localScale.x) * (direction.x >= 0 ? 1 : -1);
                spawnedBullet.transform.localScale = localScale;
            }
        }

        var damageSender = spawnedBullet.GetComponentInChildren<DamageSender>();
        if (damageSender != null)
        {
            damageSender.SetDamage(bulletDamage);
        }

        // NEW: nếu là viên đạn đặc biệt
        if (bulletName == BulletGunName.SpecialSwapBullet)
        {
            SwapTargetManager.Instance.SetSpecialBullet(spawnedBullet.transform);
        }
    }
}


