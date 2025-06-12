using UnityEngine;
using BehaviorDesigner.Runtime;

public class BehaviorAutoBinder : MonoBehaviour
{
    public BehaviorTree behaviorTree;

    void Start()
    {
        if (behaviorTree == null)
            behaviorTree = GetComponent<BehaviorTree>();

        Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (behaviorTree != null && player != null)
        {
            behaviorTree.SetVariableValue("player", player);
        }
        else
        {
            Debug.LogWarning("Không tìm thấy Player hoặc BehaviorTree để gán.");
        }
    }
}
