using UnityEngine;

public class DoorTriggerWithNPCCheck : MonoBehaviour
{
    [Header("Animator & Collider của cửa")]
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private string openTrigger = "open";
    [SerializeField] private Collider2D doorCollider;

    private bool doorOpened = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        //if (doorOpened) return;

        if (other.CompareTag("Player") && gameObject.CompareTag("mocua"))
        {
            //Debug.Log("🚪 Player đã vào vùng 'mocua'. Mở cửa!");

            if (doorAnimator != null)
                doorAnimator.SetTrigger(openTrigger);

            if (doorCollider != null)
            {
                doorCollider.isTrigger = true;
                //Debug.Log("✅ Cửa đã bật trigger.");
            }

            doorOpened = true;
        }
    }
}
