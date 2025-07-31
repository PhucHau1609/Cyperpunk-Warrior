using UnityEngine;

public class DoorTriggerWithNPCCheck : MonoBehaviour
{
    [Header("Animator & Collider của cửa")]
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private string openTrigger = "open";
    [SerializeField] private string closeTrigger = "close";
    [SerializeField] private Collider2D doorCollider;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (gameObject.CompareTag("mocua"))
        {
            if (doorAnimator != null)
            {
                doorAnimator.ResetTrigger(closeTrigger); // reset nếu cần
                doorAnimator.SetTrigger(openTrigger);
            }

            if (doorCollider != null)
                doorCollider.isTrigger = true;
        }
        else if (gameObject.CompareTag("dongcua"))
        {
            if (doorAnimator != null)
            {
                doorAnimator.ResetTrigger(openTrigger); // reset nếu cần
                doorAnimator.SetTrigger(closeTrigger);
            }

            if (doorCollider != null)
                doorCollider.isTrigger = false;
        }
    }
}

//using UnityEngine;

//public class DoorTriggerWithNPCCheck : MonoBehaviour
//{
//    [Header("Animator & Collider của cửa")]
//    [SerializeField] private Animator doorAnimator;
//    [SerializeField] private string openTrigger = "open";
//    [SerializeField] private Collider2D doorCollider;

//    private bool doorOpened = false;

//    private void OnTriggerEnter2D(Collider2D other)
//    {
//        //if (doorOpened) return;

//        if (other.CompareTag("Player") && gameObject.CompareTag("mocua"))
//        {
//            //Debug.Log("🚪 Player đã vào vùng 'mocua'. Mở cửa!");

//            if (doorAnimator != null)
//                doorAnimator.SetTrigger(openTrigger);

//            if (doorCollider != null)
//            {
//                doorCollider.isTrigger = true;
//                //Debug.Log("✅ Cửa đã bật trigger.");
//            }

//            doorOpened = true;
//        }
//    }
//}
