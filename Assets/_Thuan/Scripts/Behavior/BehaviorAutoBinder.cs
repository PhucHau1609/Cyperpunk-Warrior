using BehaviorDesigner.Runtime;
using UnityEngine;

[RequireComponent(typeof(Behavior))]
public class BehaviorAutoBinder : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float speed = 2f;

    void Start()
    {
        var behavior = GetComponent<Behavior>();
        if (behavior == null) return;

        // Gán biến "player"
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            behavior.SetVariable("player", new SharedTransform { Value = player.transform });
        }

        // Gán pointA, pointB
        if (pointA != null)
            behavior.SetVariable("pointA", new SharedTransform { Value = pointA });

        if (pointB != null)
            behavior.SetVariable("pointB", new SharedTransform { Value = pointB });

        // Gán moveSpeed
        behavior.SetVariable("moveSpeed", new SharedFloat { Value = speed });

        // Gán animator
        var anim = GetComponentInChildren<Animator>();
        if (anim != null)
            behavior.SetVariable("animator", new SharedAnimator { Value = anim });
    }
}
