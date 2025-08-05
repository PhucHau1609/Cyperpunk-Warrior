using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SkillCooldownUI : HauSingleton<SkillCooldownUI>
{
    public Image cooldownOverlay; // UI image type: Filled (Radial 360)

    private Tween cooldownTween;

    // Gọi từ bên ngoài: thời gian hiệu lực & thời gian hồi chiêu
    public void StartCooldown(float cooldownDuration)
    {
        if (cooldownTween != null && cooldownTween.IsActive())
        {
            cooldownTween.Kill();
        }

        cooldownOverlay.fillAmount = 1f;

        // DOTween: fillAmount từ 1 về 0 trong cooldownDuration
        cooldownTween = cooldownOverlay
            .DOFillAmount(0f, cooldownDuration)
            .SetEase(Ease.Linear)
            .SetUpdate(true); // chạy ngay cả khi Time.timeScale = 0
    }

    public bool IsCooldownActive()
    {
        return cooldownOverlay.fillAmount > 0f;
    }
}
