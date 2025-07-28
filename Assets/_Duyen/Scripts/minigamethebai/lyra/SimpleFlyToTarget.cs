using UnityEngine;

public class SimpleFlyToTarget : MonoBehaviour
{
    public Transform target;
    public float speed = 50f;
    public bool active = false;
    public string triggerZoneName; // Tên GameObject vùng trigger (trong scene mới)

    void Update()
    {
        if (!active || target == null) return;

        transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, target.position) < 0.05f)
        {
            transform.position = target.position;
            active = false;
            //Debug.Log("🛬 NPC đã đến đích!");
        }

        // ✅ Bật vùng trigger nếu tên hợp lệ
        if (!string.IsNullOrEmpty(triggerZoneName))
        {
            GameObject zone = GameObject.Find(triggerZoneName);
            if (zone != null)
            {
                zone.SetActive(true);
                //Debug.Log("📦 Vùng mở cửa đã được bật: " + triggerZoneName);
            }
            else
            {
                //Debug.LogWarning("❌ Không tìm thấy vùng mở cửa: " + triggerZoneName);
            }
        }
    }
}
