using UnityEngine;
using UnityEngine.AI;

public class NPCDisableTrigger2D : MonoBehaviour
{
    //
    [SerializeField] private string targetName = "FlyTarget"; // Tên GameObject trong scene hiện tại
    [SerializeField] private float flySpeed = 2f;
    //

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra xem có FloatingFollower không
        FloatingFollower follower = other.GetComponent<FloatingFollower>();
        if (follower != null)
        {
            // Tắt NavMeshAgent nếu có (NavMeshAgent vẫn là 3D)
            NavMeshAgent agent = other.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.isStopped = true;
                agent.ResetPath();
                agent.enabled = false;
            }

            // Tắt script FloatingFollower
            follower.enabled = false;

            // (Tùy chọn) Tắt Animator nếu cần
            Animator anim = other.GetComponent<Animator>();
            if (anim != null)
            {
                anim.enabled = false;
            }

            //
            // ✅ Tìm target trong scene hiện tại
            GameObject targetObj = GameObject.Find(targetName);
            if (targetObj == null)
            {
                Debug.LogWarning("❌ Không tìm thấy target trong scene hiện tại!");
                return;
            }

            // ✅ Gán target và cho NPC bay
            var flyer = other.GetComponent<SimpleFlyToTarget>();
            if (flyer != null)
            {
                flyer.target = targetObj.transform;
                flyer.speed = flySpeed;
                flyer.active = true;

                Debug.Log("🛫 NPC đã được gán target mới và bắt đầu bay.");
            }
            //

            //Debug.Log("Đã vô hiệu hóa FloatingFollower + NavMeshAgent.");
        }
    }
}
