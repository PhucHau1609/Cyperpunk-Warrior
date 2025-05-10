using UnityEngine;

public class NPCEventIcon : MonoBehaviour
{
    public Transform targetPoint; // Điểm cần đến gần (ví dụ: GameObject minigame)
    public GameObject icon;       // Icon sẽ hiện (ví dụ: dấu chấm than)

    public float showDistance = 5f; // Bán kính hiện icon

    void Update()
    {
        if (targetPoint == null || icon == null) return;

        float distance = Vector2.Distance(transform.position, targetPoint.position);

        // Hiện icon khi tới gần
        icon.SetActive(distance <= showDistance);
    }
}
