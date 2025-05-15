using UnityEngine;

public class ExtraConditionTrigger: MonoBehaviour
{
    // ✅ Event phải được khai báo public static bên ngoài hàm
    public static event System.Action OnExtraConditionMet;

    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;

        if (other.CompareTag("Player"))
        {
            triggered = true;
            Debug.Log("Extra condition met (e.g. pickup)");
            
            // ✅ Gọi event nếu có người đăng ký
            OnExtraConditionMet?.Invoke();
        }
    }
}
