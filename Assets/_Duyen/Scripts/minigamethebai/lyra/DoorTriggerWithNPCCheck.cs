using UnityEngine;

public class DoorTriggerWithNPCCheck : MonoBehaviour
{
    [Header("Kiểm tra NPC quanh cửa")]
    [SerializeField] private string npcTag = "NPC";              // Tag của NPC
    [SerializeField] private Transform doorPosition;             // Vị trí kiểm tra NPC (thường là chính cửa)
    [SerializeField] private float checkRadius = 1f;             // Bán kính kiểm tra

    [Header("Animator & Collider")]
    [SerializeField] private Animator doorAnimator;              // Animator của cửa
    [SerializeField] private string openTrigger = "open";        // Tên trigger trong Animator
    [SerializeField] private Collider2D doorCollider;            // Collider2D của cửa

    private bool doorOpened = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (doorOpened) return;

        if (other.CompareTag("Player"))
        {
            // Kiểm tra quanh cửa có NPC chưa
            Collider2D[] colliders = Physics2D.OverlapCircleAll(doorPosition.position, checkRadius);
            bool npcFound = false;

            foreach (var col in colliders)
            {
                if (col.CompareTag(npcTag))
                {
                    npcFound = true;
                    break;
                }
            }

            if (npcFound)
            {
                Debug.Log("🔓 NPC đã có mặt tại cửa. Mở cửa!");

                // Bật animation
                if (doorAnimator != null)
                    doorAnimator.SetTrigger(openTrigger);

                // Bật trigger cho collider cửa
                if (doorCollider != null)
                {
                    doorCollider.isTrigger = true;
                    Debug.Log("✅ Trigger cửa đã bật.");
                }

                doorOpened = true; // Ngăn gọi lại
            }
            else
            {
                Debug.Log("🚫 Chưa có NPC tại cửa. Không mở.");
            }
        }
    }

    // Vẽ vòng kiểm tra trong Scene
    private void OnDrawGizmosSelected()
    {
        if (doorPosition != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(doorPosition.position, checkRadius);
        }
    }
}
