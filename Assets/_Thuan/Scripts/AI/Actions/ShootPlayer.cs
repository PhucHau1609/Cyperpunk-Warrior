using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class ShootPlayer : Action
{
    public GameObject bulletPrefab;
    public float shootInterval = 1f;
    private float lastShootTime;
    private EnemyController enemy;

    public override void OnStart()
    {
        enemy = GetComponent<EnemyController>();
        lastShootTime = Time.time - shootInterval;
    }

    public override TaskStatus OnUpdate()
    {
        if (enemy == null || enemy.player == null || bulletPrefab == null)
            return TaskStatus.Failure;

        // Xoay mặt Enemy đúng hướng
        enemy.FacePlayer();

        if (Time.time >= lastShootTime + shootInterval)
        {
            enemy.animator.SetTrigger("Shoot");

            // Bắn chỉ theo trục X (trái hoặc phải)
            float xDir = (enemy.player.position.x < enemy.transform.position.x) ? -1f : 1f;
            Vector2 shootDir = new Vector2(xDir, 0f);

            // Tạo viên đạn
            GameObject bullet = GameObject.Instantiate(
                bulletPrefab,
                enemy.gunPoint.position,
                Quaternion.identity
            );

            // Gán hướng cho đạn
            Droid02Bullet bulletScript = bullet.GetComponent<Droid02Bullet>();
            if (bulletScript != null)
            {
                bulletScript.SetDirection(shootDir);
            }

            lastShootTime = Time.time;
        }

        return TaskStatus.Running;
    }
}
