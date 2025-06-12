using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectFlyToTarget : MonoBehaviour
{
    protected Transform target;
    [SerializeField] protected float speed = 34f;

    private void Update()
    {
        this.Flying();
    }

    protected virtual void Flying()
    {
        transform.parent.Translate(speed * Time.deltaTime * Vector3.right);

        /* if (this.target == null) return;

         transform.parent.Translate(speed * Time.deltaTime * Vector3.forward);*/
    }

    public virtual void SetTarget(Transform target)
    {
        this.target = target;
        transform.parent.LookAt(target);
    }
}
