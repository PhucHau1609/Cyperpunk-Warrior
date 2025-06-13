using UnityEngine;
using BehaviorDesigner.Runtime;

public class BehaviorPatrolBinder : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float patrolSpeed = 2f;
    public Animator animator;

    private Behavior behavior;

    void Start()
    {
        behavior = GetComponent<Behavior>();
        if (behavior == null)
        {
            Debug.LogWarning("Không tìm thấy BehaviorTree trên Enemy.");
            return;
        }

        behavior.SetVariableValue("pointA", pointA);
        behavior.SetVariableValue("pointB", pointB);
        behavior.SetVariableValue("moveSpeed", patrolSpeed);
        behavior.SetVariableValue("animator", animator);
    }
}
