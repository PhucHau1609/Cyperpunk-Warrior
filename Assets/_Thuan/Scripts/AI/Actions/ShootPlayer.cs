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
        
        // Đồng bộ bulletPrefab với EnemyController nếu chưa có
        if (enemy != null && enemy.bulletPrefab == null && bulletPrefab != null)
        {
            enemy.bulletPrefab = bulletPrefab;
        }
    }

    public override TaskStatus OnUpdate()
    {
        if (enemy == null || enemy.player == null)
            return TaskStatus.Failure;

        // Xoay mặt Enemy đúng hướng
        enemy.FacePlayer();

        // Chỉ trigger animation khi đủ thời gian và có thể bắn
        if (Time.time >= lastShootTime + shootInterval && enemy.canShoot)
        {
            // Trigger animation - việc bắn thực sự sẽ được thực hiện trong Animation Event
            enemy.animator.SetTrigger("Shoot");
            lastShootTime = Time.time;
        }

        return TaskStatus.Running;
    }
}