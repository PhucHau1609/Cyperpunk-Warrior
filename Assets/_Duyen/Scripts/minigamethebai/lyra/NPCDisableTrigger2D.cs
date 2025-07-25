using UnityEngine;
using UnityEngine.AI;

public class NPCDisableTrigger2D : MonoBehaviour
{
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

            Debug.Log("Đã vô hiệu hóa FloatingFollower + NavMeshAgent.");
        }
    }
}
