using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using TooltipAttribute = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;

[RequireComponent(typeof(Transform))]
[TaskCategory("Boss/Conditionals")]
public class BossCheckPlayerInRangedRange : Conditional
{
    [SerializeField]
    [Tooltip("Khoảng cách tấn công tầm xa")]
    private float _rangedAttackRange = 8f;

    [SerializeField]
    [Tooltip("Khoảng cách tấn công cận chiến")]
    private float _meleeAttackRange = 3f;

    [SerializeField]
    [Tooltip("Boss đã chuyển Phase 2 chưa")]
    private bool _isPhase2;

    [SerializeField]
    [Tooltip("Boss có thể tấn công không")]
    private bool _canAttack = true;

    // Tham chiếu tự động
    private Transform _transform;
    private Transform _playerTransform;
    private BossPhuController _bossController;

    [Tooltip("Boss đã chuyển Phase 2 chưa - từ Behavior Tree")]
    public SharedBool isPhase2SharedVar;

    public override void OnStart()
    {
        // Tự động tham chiếu transform
        _transform = GetComponent<Transform>();
        _playerTransform = GetComponent<Transform>();
        if (_bossController == null)
        {
            _bossController = transform.root.GetComponent<BossPhuController>();
        }

        // Tìm Player
        FindPlayer();
    }

    private void FindPlayer()
    {
        if (_bossController != null && _bossController.player != null)
        {
            _playerTransform = _bossController.player;
            return;
        }

        _playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    public override TaskStatus OnUpdate()
    {
        if (_playerTransform == null)
        {
            FindPlayer();
            if (_playerTransform == null)
            {
                return TaskStatus.Failure;
            }
        }

        // Kiểm tra điều kiện tấn công
        if (!_canAttack) return TaskStatus.Failure;

        // Kiểm tra Phase 2 - Ưu tiên từ BossController
        bool isPhase2 = GetPhase2Status();

        // Tính khoảng cách
        float distance = Vector2.Distance(_transform.position, _playerTransform.position);

        // Debug log để kiểm tra
        if (Application.isEditor)
        {
            //Debug.Log($"BossCheckPlayerInRangedRange - IsPhase2: {isPhase2}, Distance: {distance:F2}, RangedRange: {_rangedAttackRange}, MeleeRange: {_meleeAttackRange}");
        }

        // Chỉ tấn công tầm xa khi:
        // 1. Boss ở Phase 2
        // 2. Player ở trong tầm bắn nhưng ngoài tầm cận chiến
        if (isPhase2 && distance <= _rangedAttackRange && distance > _meleeAttackRange)
        {
            Debug.Log("Boss có thể tấn công tầm xa!");
            return TaskStatus.Success;
        }

        return TaskStatus.Failure;
    }

    private bool GetPhase2Status()
    {
        // Ưu tiên 1: Từ BossPhuController
        if (_bossController != null)
        {
            return _bossController.isPhase2;
        }

        // Ưu tiên 2: Từ Shared Variable
        if (isPhase2SharedVar != null)
        {
            return isPhase2SharedVar.Value;
        }

        // Fallback: false
        return false;
    }

    // Phương thức để cập nhật trạng thái Phase 2 từ bên ngoài
    public void SetPhase2(bool isPhase2)
    {
        if (isPhase2SharedVar != null)
        {
            isPhase2SharedVar.Value = isPhase2;
        }

        //Debug.Log($"BossCheckPlayerInRangedRange - SetPhase2: {isPhase2}");
    }

    // Phương thức để cập nhật khả năng tấn công
    public void SetCanAttack(bool canAttack)
    {
        _canAttack = canAttack;
    }
    
    public override void OnAwake()
    {
        base.OnAwake();
        
        // Lấy ranges từ BossController nếu có
        if (_bossController != null)
        {
            _rangedAttackRange = _bossController.rangedAttackRange;
            _meleeAttackRange = _bossController.meleeAttackRange;
        }
    }
}