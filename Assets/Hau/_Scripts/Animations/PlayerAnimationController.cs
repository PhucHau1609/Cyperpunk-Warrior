using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    private Animator animator;
    private PlayerAnimationState currentState;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void ChangeState(PlayerAnimationState newState)
    {
        if (newState == currentState) return;

        currentState = newState;

        // Option 1: Trigger-based animation
        animator.Play(newState.ToString());

        // Option 2: Nếu dùng bool/trigger:
        ResetAllBools();
        animator.SetBool(newState.ToString(), true);
    }

    private void ResetAllBools()
    {
        foreach (PlayerAnimationState state in System.Enum.GetValues(typeof(PlayerAnimationState)))
        {
            animator.SetBool(state.ToString(), false);
        }
    }

    public void SetFloat(string name, float value) => animator.SetFloat(name, value);
    public void SetBool(string name, bool value) => animator.SetBool(name, value);
    public void Trigger(string name) => animator.SetTrigger(name);

}
