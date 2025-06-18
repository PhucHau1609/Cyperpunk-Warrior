using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectFlyToTarget : MonoBehaviour
{
    protected Transform target;
    [SerializeField] protected float speed = 34f;

    [SerializeField] protected Vector2 direction = Vector2.right;


    private void Update()
    {
        this.Flying();
    }

    protected virtual void Flying()
    {
        transform.parent.Translate(direction.normalized * speed * Time.deltaTime);
    }


    public virtual void SetTarget(Transform target)
    {
        this.target = target;
        transform.parent.LookAt(target);
    }

    public void SetDirection(Vector2 dir)
    {
        this.direction = dir.normalized;

        // Flip Model theo hướng X (chỉ flip trái-phải)
        Transform model = transform.parent.Find("Model");
        if (model != null)
        {
            Vector3 scale = model.localScale;
            scale.x = Mathf.Abs(scale.x) * (dir.x >= 0 ? 1 : -1);
            model.localScale = scale;
        }

        // Quay đầu viên đạn theo hướng
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.parent.rotation = Quaternion.Euler(0f, 0f, angle);
    }


}
