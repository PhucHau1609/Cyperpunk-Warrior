using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class DropBombs : Action
{
    public GameObject bombPrefab;
    public Transform dropPoint;
    public SharedAnimator animator; 

    private int bombsDropped = 0;
    private float interval = 0.3f;
    private float lastDropTime;

    public override void OnStart()
    {
        bombsDropped = 0;
        lastDropTime = Time.time - interval;
    }

    public override TaskStatus OnUpdate()
    {
        if (bombsDropped >= 5)
            return TaskStatus.Success;

        if (Time.time >= lastDropTime + interval)
        {
            GameObject.Instantiate(bombPrefab, dropPoint.position, Quaternion.identity);

            if (animator != null && animator.Value != null)
            {
                animator.Value.SetTrigger("DropBomb");
            }

            bombsDropped++;
            lastDropTime = Time.time;
        }

        return TaskStatus.Running;
    }
}
