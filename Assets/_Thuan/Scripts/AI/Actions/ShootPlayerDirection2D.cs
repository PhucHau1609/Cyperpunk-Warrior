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
    private Transform cachedPlayerTransform;
    private Transform cachedGunPointTransform;

    public override void OnStart()
    {
        // Đặt thời gian bắn ban đầu
        lastShootTime = Time.time - shootInterval;

        // Tự động gán giá trị nếu chưa được gán
        AutoAssignReferences();
    }

    private void AutoAssignReferences()
    {
        // Tìm Player
        if (player == null || player.Value == null)
        {
            cachedPlayerTransform = FindPlayerInScene();
            if (cachedPlayerTransform != null)
            {
                player.Value = cachedPlayerTransform;
            }
        }

        // Tìm GunPoint
        if (gunPoint == null || gunPoint.Value == null)
        {
            cachedGunPointTransform = FindGunPointInParent();
            if (cachedGunPointTransform != null)
            {
                gunPoint.Value = cachedGunPointTransform;
            }
        }

        // Tìm Animator
        if (animator == null || animator.Value == null)
        {
            animator.Value = GetComponent<Animator>();
        }

        // Tìm BulletPrefab nếu chưa gán
        if (bulletPrefab == null)
        {
            bulletPrefab = Resources.Load<GameObject>("Prefabs/BulletPrefab");
        }
    }

    private Transform FindPlayerInScene()
    {
        // Tương tự phương thức ở FacePlayer
        GameObject playerObject = null;
        playerObject = GameObject.FindGameObjectWithTag("Player");
        
        if (playerObject == null)
        {
            playerObject = GameObject.Find("Player");
        }

        return playerObject?.transform;
    }

    private Transform FindGunPointInParent()
    {
        // Tìm GunPoint trong parent hoặc children
        Transform gunPointTransform = transform.Find("GunPoint");
        
        if (gunPointTransform == null)
        {
            gunPointTransform = transform.parent?.Find("GunPoint");
        }

        return gunPointTransform;
    }

    public override TaskStatus OnUpdate()
    {
        // Kiểm tra các tham chiếu
        if (player == null || player.Value == null || 
            gunPoint == null || gunPoint.Value == null || 
            bulletPrefab == null)
        {
            // Thử gán lại nếu thiếu
            AutoAssignReferences();
            return TaskStatus.Failure;
        }

        // Logic bắn đạn như cũ
        if (Time.time >= lastShootTime + shootInterval)
        {
            animator?.Value?.SetTrigger("Shoot");

            Vector2 shootDir = (player.Value.position - gunPoint.Value.position).normalized;

            GameObject bullet = GameObject.Instantiate(
                bulletPrefab,
                gunPoint.Value.position,
                Quaternion.identity
            );

            var bulletScript = bullet.GetComponent<Droid01Bullet>();
            if (bulletScript != null)
                bulletScript.SetDirection(shootDir);

            lastShootTime = Time.time;

            Debug.DrawLine(gunPoint.Value.position, player.Value.position, Color.red, 1f);
        }

        return TaskStatus.Running;
    }
}