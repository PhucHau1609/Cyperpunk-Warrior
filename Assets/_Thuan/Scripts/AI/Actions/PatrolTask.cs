using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class PatrolTask : Action
{
    public enum MoveMode { TransformPosition, RigidbodyVelocity }

    [Header("Setup")]
    public MoveMode movementType = MoveMode.TransformPosition;

    public SharedTransform pointA;
    public SharedTransform pointB;
    public SharedFloat moveSpeed;
    public SharedAnimator animator;

    private Transform currentTarget;
    private Rigidbody2D rb;

    public override void OnStart()
    {
        currentTarget = pointA.Value;

        if (movementType == MoveMode.RigidbodyVelocity)
            rb = GetComponent<Rigidbody2D>();
    }

    public override TaskStatus OnUpdate()
    {
        if (pointA.Value == null || pointB.Value == null)
            return TaskStatus.Failure;

        Vector3 targetPos = currentTarget.position;
        float dist = Vector2.Distance(transform.position, targetPos);
        Vector2 dir = (targetPos - transform.position).normalized;

        switch (movementType)
        {
            case MoveMode.TransformPosition:
                transform.position = Vector2.MoveTowards(transform.position, targetPos, moveSpeed.Value * Time.deltaTime);
                break;

            case MoveMode.RigidbodyVelocity:
                if (rb == null)
                {
                    rb = GetComponent<Rigidbody2D>();
                    if (rb == null)
                        return TaskStatus.Failure;
                }

                rb.linearVelocity = dir * moveSpeed.Value;
                break;
        }

        // Flip mặt
        if (dir.x != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Sign(dir.x) * Mathf.Abs(scale.x);
            transform.localScale = scale;
        }

        animator?.Value?.SetBool("Run", true);
        animator?.Value?.SetBool("Fly", movementType == MoveMode.RigidbodyVelocity);

        // Đổi target nếu tới nơi
        if (dist < 0.2f)
        {
            currentTarget = (currentTarget == pointA.Value) ? pointB.Value : pointA.Value;
        }

        return TaskStatus.Running;
    }

    public override void OnEnd()
    {
        if (movementType == MoveMode.RigidbodyVelocity && rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        animator?.Value?.SetBool("Run", false);
        animator?.Value?.SetBool("Fly", false);
    }
}