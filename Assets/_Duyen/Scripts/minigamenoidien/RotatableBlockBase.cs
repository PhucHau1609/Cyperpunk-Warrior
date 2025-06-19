using DG.Tweening;
using UnityEngine;

public abstract class RotatableBlockBase : MonoBehaviour
{
    protected bool isRotating = false;
    protected Tween rotateTween;

    protected void Rotate(float angle = -90f, float duration = 0.25f, Ease ease = Ease.OutCubic, System.Action onComplete = null)
    {
        if (isRotating || (rotateTween != null && rotateTween.IsActive())) return;

        isRotating = true;
        rotateTween = transform
            .DORotate(transform.eulerAngles + new Vector3(0, 0, angle), duration)
            .SetEase(ease)
            .OnComplete(() =>
            {
                isRotating = false;
                rotateTween = null;
                AudioManager.Instance?.PlayBlockInteractSFX();
                MinigameManager.Instance?.CheckLevel();
                onComplete?.Invoke();
            });
    }
}