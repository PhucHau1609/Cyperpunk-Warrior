using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using TooltipAttribute = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;

[RequireComponent(typeof(Transform))]
[TaskCategory("Boss/Conditionals")]
public class BossCheckPlayerInMeleeRange : Conditional
{
    [SerializeField]
    [Tooltip("Khoảng cách tấn công cận chiến")]
    private float _meleeAttackRange = 3f;
    
    [SerializeField]
    [Tooltip("Boss có thể tấn công không")]
    private bool _canAttack = true;

    // Tham chiếu tự động
    private Transform _transform;
    private Transform _playerTransform;

    public override void OnStart()
    {
        // Tự động tham chiếu transform
        _transform = GetComponent<Transform>();

        // Tìm Player
        FindPlayer();
    }

    private void FindPlayer()
    {
        // Ưu tiên tìm Player qua tag
        _playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Nếu không tìm thấy, thử tìm qua script PlayerController
        // if (_playerTransform == null)
        // {
        //     var playerController = FindObjectOfType<PlayerController>();
        //     if (playerController != null)
        //     {
        //         _playerTransform = playerController.transform;
        //     }
        // }
    }
    
    public override TaskStatus OnUpdate()
    {
        // Kiểm tra Player và khả năng tấn công
        if (_playerTransform == null || !_canAttack) 
        {
            // Thử tìm lại Player nếu mất
            FindPlayer();
            return TaskStatus.Failure;
        }
        
        // Tính khoảng cách
        float distance = Vector2.Distance(_transform.position, _playerTransform.position);
        
        // Tấn công cận chiến khi Player ở gần
        if (distance <= _meleeAttackRange)
        {
            return TaskStatus.Success;
        }
        
        return TaskStatus.Failure;
    }

    // Phương thức để điều chỉnh khoảng cách tấn công từ bên ngoài
    public void SetMeleeAttackRange(float range)
    {
        _meleeAttackRange = range;
    }

    // Phương thức để điều chỉnh khả năng tấn công từ bên ngoài
    public void SetCanAttack(bool canAttack)
    {
        _canAttack = canAttack;
    }

    // Phương thức để cập nhật vị trí Player thủ công (nếu cần)
    public void SetPlayerTransform(Transform playerTransform)
    {
        _playerTransform = playerTransform;
    }
}