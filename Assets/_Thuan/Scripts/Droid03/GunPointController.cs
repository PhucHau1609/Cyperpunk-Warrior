using UnityEngine;

[ExecuteAlways]
public class GunPointController : MonoBehaviour
{
    public Transform enemy;
    public Vector2 offset = new Vector2(0.5f, 0.1f);

    private void LateUpdate()
    {
        if (enemy == null)
            enemy = transform.parent;
        if (enemy == null) return;

        // Luôn gán vị trí theo chiều dương, không phụ thuộc scale.x
        float dir = Mathf.Sign(enemy.localScale.x);
        transform.localPosition = new Vector3(offset.x, offset.y, 0);

        // Bù lại flip của Enemy bằng cách đảo scale X
        Vector3 scale = transform.localScale;
        scale.x = dir; // <- đảo ngược lại flip của cha
        transform.localScale = scale;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.right * 0.5f);
    }
}
