using UnityEngine;

public class WeaponShooter : MonoBehaviour
{
    [SerializeField] private Transform firePoint; // Nòng súng
    [SerializeField] private BulletGunName bulletName; // Tên đạn trong Pool
    [SerializeField] private int bulletDamage = 1; // Có thể config nếu muốn

    private int lastDirection = 1; // 1: phải, -1: trái

    [SerializeField] private ParticleSystem shellParticle;
    [SerializeField] private ParticleSystem flameParticle;

    private void Update()
    {
        // Chỉ bắn khi vũ khí này đang được bật
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

        // Spawn từ EffectSpawner
        EffectCtrl bulletBase = EffectSpawnerCtrl.Instance.EffectSpawner.PoolPrefabs.GetPrefabByName(bulletName.ToString());
        EffectCtrl spawnedBullet = EffectSpawnerCtrl.Instance.EffectSpawner.Spawn(bulletBase, firePoint.position);

        //Debug.Log($"[Shoot] Expecting Bullet: {bulletName}");

        if (spawnedBullet == null)
        {
            Debug.LogWarning("Không tìm thấy hoặc spawn được viên đạn.");
            return;
        }

        spawnedBullet.gameObject.SetActive(true);

        // Gán hướng bay
        Vector3 shootDir = firePoint.TransformDirection(Vector3.right).normalized;

        // Flip SpriteRenderer nếu có

        if (spawnedBullet is EffectFlyAbstract effectFly)
        {
            if (effectFly.EffectFlyToTarget != null)
            {
                // Hướng bay

                float angle = Mathf.Atan2(shootDir.y, shootDir.x) * Mathf.Rad2Deg;
                spawnedBullet.transform.rotation = Quaternion.Euler(0, 0, angle);

                lastDirection = shootDir.x >= 0 ? 1 : -1;

                SpriteRenderer spriteRenderer = spawnedBullet.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.flipX = shootDir.x < 0;
                }


                Vector3 localScale = spawnedBullet.transform.localScale;
                localScale.x = Mathf.Abs(localScale.x) * lastDirection;
                spawnedBullet.transform.localScale = localScale;
            }
        }

        // Gán damage nếu cần
        var damageSender = spawnedBullet.GetComponentInChildren<DamageSender>();
        if (damageSender != null)
        {
            damageSender.SetDamage(bulletDamage);
        }



        // 🔥 Bắn hiệu ứng
        if (shellParticle != null) shellParticle.Play();
        if (flameParticle != null) flameParticle.Play();
    }
}



/* if (spawnedBullet is EffectFlyAbstract effectFly)
 {
     if (effectFly.EffectFlyToTarget != null)
     {
         // Hướng bay
         Vector2 dir = shootDir.x >= 0 ? Vector2.right : Vector2.left;
         effectFly.EffectFlyToTarget.SetDirection(dir);
         */
/*  float angle = Mathf.Atan2(shootDir.y, shootDir.x) * Mathf.Rad2Deg;
           spawnedBullet.transform.rotation = Quaternion.Euler(0, 0, angle);

           lastDirection = shootDir.x >= 0 ? 1 : -1;

           Vector3 localScale = spawnedBullet.transform.localScale;
           localScale.x = Mathf.Abs(localScale.x) * lastDirection;
           spawnedBullet.transform.localScale = localScale;*/

