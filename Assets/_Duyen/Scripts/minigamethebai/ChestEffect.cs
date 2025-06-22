using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ChestEffect : MonoBehaviour
{
    [Header("Images")]
    public Image chestImage;
    public Image burstImage;

    private Outline chestOutline;
    private Tween burstScaleTween;
    private Tween burstRotateTween;
    private Tween outlineBlinkTween;

    void Awake()
    {
        chestOutline = chestImage.GetComponent<Outline>();
        if (chestOutline != null)
        {
            chestOutline.enabled = false;
            chestOutline.effectColor = new Color(1f, 1f, 1f, 1f); // Trắng đậm
        }
    }

    void OnEnable()
    {
        PlayChestEffect();
    }

    void OnDisable()
    {
        StopAllEffects();
    }

    public void PlayChestEffect()
    {
        chestImage.transform.localScale = Vector3.one * 0.5f;
        burstImage.transform.localScale = Vector3.one * 0.8f;
        burstImage.color = Color.white;
        burstImage.transform.rotation = Quaternion.identity;

        if (chestOutline != null)
        {
            chestOutline.enabled = true;

            // Viền trắng nhấp nháy bằng cách tween alpha
            outlineBlinkTween = DOTween.ToAlpha(
                () => chestOutline.effectColor,
                c => chestOutline.effectColor = c,
                0f, 0.5f
            )
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(true);
        }

        // Hiệu ứng scale rương
        chestImage.transform
            .DOScale(1.5f, 1.2f)
            .SetEase(Ease.OutBack)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                chestImage.transform.DOScale(1f, 0.8f).SetEase(Ease.InOutSine).SetUpdate(true);
            });

        // Hiệu ứng burst: scale và xoay
        burstScaleTween = burstImage.transform
            .DOScale(2f, 1.2f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(true);

        burstRotateTween = burstImage.transform
            .DORotate(new Vector3(0f, 0f, 360f), 6f, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1)
            .SetUpdate(true);
    }

    public void StopAllEffects()
    {
        burstScaleTween?.Kill();
        burstRotateTween?.Kill();
        outlineBlinkTween?.Kill();

        burstImage.transform.localScale = Vector3.one;
        burstImage.transform.rotation = Quaternion.identity;

        if (chestOutline != null)
        {
            chestOutline.enabled = false;
            chestOutline.effectColor = new Color(1f, 1f, 1f, 1f); // Reset trắng đậm
        }
    }
}
