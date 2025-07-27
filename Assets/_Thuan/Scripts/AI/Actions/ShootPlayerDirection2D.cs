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
    private FlyingDroidController droidController; // Thêm reference

    public override void OnStart()
    {
        // Đặt thời gian bắn ban đầu
        lastShootTime = Time.time - shootInterval;

        // Lấy reference đến FlyingDroidController
        droidController = GetComponent<FlyingDroidController>();

        // Dừng di chuyển và quay mặt về Player
        if (droidController != null)
        {
            droidController.Stop();
            droidController.FacePlayer();
            
            // Đồng bộ bulletPrefab với FlyingDroidController nếu chưa có
            if (droidController.bulletPrefab == null && bulletPrefab != null)
            {
                droidController.bulletPrefab = bulletPrefab;
            }
        }

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
            gunPoint == null || gunPoint.Value == null)
        {
            // Thử gán lại nếu thiếu
            AutoAssignReferences();
            return TaskStatus.Failure;
        }

        // Luôn quay mặt về Player khi đang bắn
        if (droidController != null)
        {
            droidController.FacePlayer();
        }

        // Logic bắn đạn - chỉ trigger animation khi đủ thời gian và có thể bắn
        if (Time.time >= lastShootTime + shootInterval && 
            droidController != null && droidController.canShoot)
        {
            // Trigger animation - việc bắn thực sự sẽ được thực hiện trong Animation Event
            animator?.Value?.SetTrigger("Shoot");
            lastShootTime = Time.time;
            
            Debug.Log($"[{gameObject.name}] Triggered Flying Droid Shoot animation at {Time.time}");
            Debug.DrawLine(gunPoint.Value.position, player.Value.position, Color.red, 1f);
        }

        return TaskStatus.Running;
    }

    public override void OnEnd()
    {
        // Khi kết thúc bắn, cho phép droid tiếp tục di chuyển
        if (droidController != null)
        {
            droidController.Resume();
        }
    }
}