using UnityEngine;

public class AnimationTester : MonoBehaviour
{
    public Animator playerAnimator;

    public void PlayAnimation(string animationName)
    {
        playerAnimator.Play(animationName);
    }
}
