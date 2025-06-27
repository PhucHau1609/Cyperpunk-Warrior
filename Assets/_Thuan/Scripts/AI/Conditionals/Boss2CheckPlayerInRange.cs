using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using TooltipAttribute = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;

[RequireComponent(typeof(Transform))]
[TaskCategory("Boss2/Conditionals")]
public class Boss2CheckPlayerInRange : Conditional
{
    [SerializeField]
    [Tooltip("Khoảng cách tấn công tầm xa")]
    private float _rangedAttackRange = 10f;
    
    [SerializeField]
    [Tooltip("Boss có thể tấn công không")]
    private bool _canAttack = true;
    
    // Tham chiếu
    private Transform _transform;
    private Transform _playerTransform;
    private Boss2Controller _boss2Controller;
    
    public override void OnStart()
    {
        _transform = GetComponent<Transform>();
        
        if (_boss2Controller == null)
        {
            _boss2Controller = GetComponent<Boss2Controller>();
        }
        
        // Tìm Player từ Boss2Controller (auto-find)
        FindPlayer();
    }
    
    private void FindPlayer()
    {
        if (_boss2Controller != null && _boss2Controller.player != null)
        {
            _playerTransform = _boss2Controller.player;
            return;
        }
        
        // Fallback: tìm bằng tag
        _playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
    }
    
    public override TaskStatus OnUpdate()
    {
        // Luôn tìm player mới từ Boss2Controller
        if (_boss2Controller != null && _boss2Controller.player != null)
        {
            _playerTransform = _boss2Controller.player;
        }
        else if (_playerTransform == null)
        {
            FindPlayer();
            if (_playerTransform == null)
            {
                return TaskStatus.Failure;
            }
        }
        
        // Kiểm tra điều kiện tấn công
        if (!_canAttack) return TaskStatus.Failure;
        
        // Tính khoảng cách
        float distance = Vector2.Distance(_transform.position, _playerTransform.position);
        
        // Lấy range từ Boss2Controller nếu có
        //float actualRange = _boss2Controller != null ? _boss2Controller.RangedAttackRange : _rangedAttackRange;
        
        // if (distance <= actualRange)
        // {
        //     return TaskStatus.Success;
        // }
        
        return TaskStatus.Failure;
    }
    
    public void SetCanAttack(bool canAttack)
    {
        _canAttack = canAttack;
    }
}