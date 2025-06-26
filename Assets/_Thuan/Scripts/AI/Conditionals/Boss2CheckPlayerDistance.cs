using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using TooltipAttribute = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;

[TaskCategory("Boss2/Conditionals")]
public class Boss2CheckPlayerDistance : Conditional
{
    [Tooltip("Khoảng cách cần kiểm tra")]
    public float targetDistance = 5f;
    
    [Tooltip("Kiểm tra nhỏ hơn hoặc bằng (true) hay lớn hơn (false)")]
    public bool checkLessOrEqual = true;
    
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
        
        FindPlayer();
    }
    
    private void FindPlayer()
    {
        if (_boss2Controller != null && _boss2Controller.player != null)
        {
            _playerTransform = _boss2Controller.player;
            return;
        }
        
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
        
        float distance = Vector2.Distance(_transform.position, _playerTransform.position);
        
        bool result = checkLessOrEqual ? 
            distance <= targetDistance : 
            distance > targetDistance;
            
        return result ? TaskStatus.Success : TaskStatus.Failure;
    }
}