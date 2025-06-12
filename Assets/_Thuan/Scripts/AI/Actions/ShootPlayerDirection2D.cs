using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class ShootPlayerDirection2D : Action
{
    [Header("References")]
    public SharedTransform player;
    public SharedTransform gunPoint;
    public SharedAnimator animator;

    [Header("Bullet")]
    public GameObject bulletPrefab;
    public float shootInterval = 1f;

    private float lastShootTime;

    public override void OnStart()
    {
        lastShootTime = Time.time - shootInterval;
    }

    public override TaskStatus OnUpdate()
    {
        if (player == null || player.Value == null || gunPoint == null || gunPoint.Value == null || bulletPrefab == null)
            return TaskStatus.Failure;

        if (Time.time >= lastShootTime + shootInterval)
        {
            animator?.Value?.SetTrigger("Shoot");

            // Hướng bắn theo Player
            Vector2 shootDir = (player.Value.position - gunPoint.Value.position).normalized;

            // Tạo đạn
            GameObject bullet = GameObject.Instantiate(
                bulletPrefab,
                gunPoint.Value.position,
                Quaternion.identity
            );

            // Gán hướng bay
            var bulletScript = bullet.GetComponent<Droid01Bullet>();
            if (bulletScript != null)
                bulletScript.SetDirection(shootDir);

            lastShootTime = Time.time;

            Debug.DrawLine(gunPoint.Value.position, player.Value.position, Color.red, 1f);
            //Debug.Log("[Shoot] Dir: " + shootDir);
        }

        return TaskStatus.Running;
    }
}
