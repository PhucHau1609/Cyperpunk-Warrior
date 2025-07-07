using UnityEngine;

public class BombAnimatorController : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // Gọi từ animation event
    public void TriggerBoom()
    {
        animator.SetTrigger("Explode");
    }
}
