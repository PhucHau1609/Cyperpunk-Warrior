using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using TooltipAttribute = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;

[RequireComponent(typeof(Transform))]
[TaskCategory("Boss/Actions")]
public class BossFacePlayer : Action
{
    // Tham chiếu tự động
    private Transform _transform;
    private Transform _playerTransform;

    public override void OnStart()
    {
        // Tự động tham chiếu transform của Boss
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
            // Thử tìm lại Player nếu mất
            FindPlayer();
            return TaskStatus.Failure;
        }
        
        // Xoay Boss về phía Player
        if (_playerTransform.position.x < _transform.position.x)
        {
            _transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            _transform.localScale = new Vector3(1, 1, 1);
        }
        
        return TaskStatus.Success;
    }

    // Phương thức để cập nhật vị trí Player thủ công (nếu cần)
    public void SetPlayerTransform(Transform playerTransform)
    {
        _playerTransform = playerTransform;
    }
}