using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using TooltipAttribute = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
[TaskCategory("Boss/Actions")]
public class BossChasePlayer : Action
{
    [SerializeField]
    [Tooltip("Tốc độ di chuyển")]
    private float _moveSpeed = 3f;
    
    [SerializeField]
    [Tooltip("Khoảng cách dừng lại")]
    private float _stopDistance = 1f;

    // Tham chiếu tự động
    private Rigidbody2D _rigidbody;
    private Animator _animator;
    private Transform _transform;
    private Transform _playerTransform;

    public override void OnStart()
    {
        // Tự động tham chiếu các component
        _rigidbody = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _transform = GetComponent<Transform>();

        // Tìm Player
        FindPlayer();
    }

    private void FindPlayer()
    {
        // Ưu tiên tìm Player qua tag
        _playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
    }
    
    public override TaskStatus OnUpdate()
    {
        // Kiểm tra Player
        if (_playerTransform == null)
        {
            FindPlayer();
            return TaskStatus.Failure;
        }
        
        float distanceToPlayer = Vector2.Distance(_transform.position, _playerTransform.position);
        
        // Nếu đã đủ gần thì dừng lại
        if (distanceToPlayer <= _stopDistance)
        {
            StopMoving();
            return TaskStatus.Success;
        }
        
        // Tính toán hướng di chuyển
        Vector2 direction = (_playerTransform.position - _transform.position).normalized;
        
        // Di chuyển
        MoveTowardsPlayer(direction);
        
        return TaskStatus.Running;
    }
    
    private void StopMoving()
    {
        if (_rigidbody != null)
        {
            _rigidbody.linearVelocity = new Vector2(0, _rigidbody.linearVelocity.y);
        }
        
        if (_animator != null)
        {
            _animator.SetBool("IsRunning", false);
        }
    }

    private void MoveTowardsPlayer(Vector2 direction)
    {
        if (_rigidbody != null)
        {
            _rigidbody.linearVelocity = new Vector2(direction.x * _moveSpeed, _rigidbody.linearVelocity.y);
        }
        
        if (_animator != null)
        {
            _animator.SetBool("IsRunning", true);
        }
    }
    
    public override void OnEnd()
    {
        StopMoving();
    }

    // Các phương thức hỗ trợ điều chỉnh
    public void SetMoveSpeed(float speed)
    {
        _moveSpeed = speed;
    }

    public void SetStopDistance(float distance)
    {
        _stopDistance = distance;
    }

    public void SetPlayerTransform(Transform playerTransform)
    {
        _playerTransform = playerTransform;
    }
}