using UnityEngine;

public class SimpleFlyToTarget : MonoBehaviour
{
    public Transform target;
    public float speed = 10f;
    public bool active = false;

    void Update()
    {
        if (!active || target == null) return;

        transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, target.position) < 0.05f)
        {
            transform.position = target.position;
            active = false;
            Debug.Log("🛬 NPC đã đến đích!");
        }
    }
}
