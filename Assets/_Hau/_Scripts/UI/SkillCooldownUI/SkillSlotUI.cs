using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using TMPro;
using System.Threading.Tasks;
using UnityEngine.EventSystems; // <- thêm

public class SkillSlotUI : MonoBehaviour
{
    [Header("Optional Image Display")]
    public Image iconImage;
    public Image cooldownOverlay;   // vòng hồi chiêu (outer ring)

    [Header("Optional Duration Display")]
    public Image durationOverlay;   // vòng thời lượng (inner ring)
    public TextMeshProUGUI durationText; // số giây đếm ngược (optional)

    [Header("Optional Text Display")]
    public TextMeshProUGUI keyText;

    [Header("Button Click Skill")]
    public Button clickableButton;

    private SkillData skill;
    private bool isCoolingDown = false;
    private Tween durationTween;
    private Coroutine durationCo;

    public void Setup(SkillData data)
    {
        skill = data;
        if (iconImage) iconImage.sprite = skill.icon;
        if (cooldownOverlay) cooldownOverlay.fillAmount = 0f;

        if (keyText != null) keyText.text = FormatKeyCode(skill.triggerKey);

        if (clickableButton != null)
        {
            clickableButton.onClick.RemoveAllListeners();
            clickableButton.onClick.AddListener(() => TryActivate());
        }

        // Ẩn UI thời lượng lúc đầu
        if (durationOverlay) durationOverlay.gameObject.SetActive(false);
        if (durationText) durationText.gameObject.SetActive(false);
    }

    public async void TryActivate()
    {
        if (isCoolingDown) return;

        if (skill.onActivateCallback != null && skill.onActivateCallback.Invoke())
        {
            // BẮT ĐẦU UI thời lượng (nếu skill có duration > 0)
            if (durationOverlay && skill.effectDuration > 0f)
                StartDurationUI(skill.effectDuration);

            await HandleCooldownAsync();
        }
    }

    private async Task HandleCooldownAsync()
    {
        isCoolingDown = true;

        if (cooldownOverlay)
        {
            cooldownOverlay.DOKill();
            cooldownOverlay.fillAmount = 1f;
            cooldownOverlay.DOFillAmount(0f, skill.cooldownTime).SetEase(Ease.Linear);
        }

        await Task.Delay((int)(skill.cooldownTime * 1000));
        isCoolingDown = false;
    }

    // --- xử lý click chuột / touch ---
    public void OnPointerClick(PointerEventData eventData)
    {
        // left click / tap -> kích hoạt skill
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            TryActivate();
        }
    }

    private void StartDurationUI(float seconds)
    {
        // Dọn dẹp trước
        if (durationTween != null && durationTween.IsActive()) durationTween.Kill();
        if (durationCo != null) StopCoroutine(durationCo);

        durationOverlay.gameObject.SetActive(true);
        durationOverlay.fillAmount = 1f;

        durationTween = durationOverlay
            .DOFillAmount(0f, seconds)
            .SetEase(Ease.Linear)
            .OnComplete(() => durationOverlay.gameObject.SetActive(false));

        if (durationText)
        {
            durationText.gameObject.SetActive(true);
            durationCo = StartCoroutine(UpdateDurationText(seconds));
        }
    }

    private IEnumerator UpdateDurationText(float seconds)
    {
        float t = seconds;
        while (t > 0f)
        {
            // Hiển thị số giây còn lại (làm tròn lên cho “đã mắt”)
            durationText.text = Mathf.CeilToInt(t).ToString();
            t -= Time.deltaTime;
            yield return null;
        }
        durationText.gameObject.SetActive(false);
        durationCo = null;
    }

    private void OnDisable()
    {
        // cleanup tween/coroutine khi object bị disable
        if (durationTween != null && durationTween.IsActive()) durationTween.Kill();
        if (durationCo != null) { StopCoroutine(durationCo); durationCo = null; }
    }

    private string FormatKeyCode(KeyCode key)
    {
        string keyString = key.ToString();

        // Nếu là Alpha1 → 1, Alpha2 → 2, v.v...
        if (key >= KeyCode.Alpha0 && key <= KeyCode.Alpha9)
        {
            return keyString.Replace("Alpha", "");
        }

        // Nếu là Keypad1 → 1
        if (key >= KeyCode.Keypad0 && key <= KeyCode.Keypad9)
        {
            return keyString.Replace("Keypad", "");
        }

        // Nếu là Space → "␣", Escape → "Esc", v.v (tuỳ chọn)
        if (key == KeyCode.Space) return "␣";
        if (key == KeyCode.Escape) return "Esc";

        return keyString;
    }

    // ... (FormatKeyCode giữ nguyên)

    public void ForceHideAndReset()
    {
        // Dừng tween/coroutine
        if (durationTween != null && durationTween.IsActive()) durationTween.Kill();
        if (durationCo != null) { StopCoroutine(durationCo); durationCo = null; }

        // Ẩn các overlay/text
        if (durationOverlay) durationOverlay.gameObject.SetActive(false);
        if (durationText) durationText.gameObject.SetActive(false);

        // Reset cooldown overlay để khi hiện lại không bị nửa vời
        if (cooldownOverlay)
        {
            cooldownOverlay.DOKill();
            cooldownOverlay.fillAmount = 0f;
        }

        // Optional: khóa nút nếu muốn
        if (clickableButton) clickableButton.interactable = true; // hoặc false nếu bạn muốn
    }
}


/*using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using TMPro;
using System.Threading.Tasks;
using UnityEngine.EventSystems; // <- thêm


public class SkillSlotUI : MonoBehaviour
{
    [Header("Optional Image Display")]
    public Image iconImage;
    public Image cooldownOverlay;

    [Header("Optional Text Display")]
    public TextMeshProUGUI keyText;

    [Header("Button Click Skill")]
    public Button clickableButton; // kéo drop vào prefab


    private SkillData skill;
    private bool isCoolingDown = false;

    public void Setup(SkillData data)
    {
        skill = data;
        iconImage.sprite = skill.icon;
        cooldownOverlay.fillAmount = 0f;

        if (keyText != null)
        {
            keyText.text = FormatKeyCode(skill.triggerKey); // ✅ dùng hàm chuyển đổi
        }

        if (clickableButton != null)
        {
            clickableButton.onClick.RemoveAllListeners();
            clickableButton.onClick.AddListener(() => TryActivate());
        }
    }

    public async void TryActivate()
    {
        if (isCoolingDown) return;

        if (skill.onActivateCallback != null && skill.onActivateCallback.Invoke())
        {
            await HandleCooldownAsync(); // dùng Task thay cho coroutine
        }
    }

    private async Task HandleCooldownAsync()
    {
        isCoolingDown = true;
        cooldownOverlay.fillAmount = 1f;
        cooldownOverlay.DOFillAmount(0f, skill.cooldownTime).SetEase(Ease.Linear);

        await Task.Delay((int)(skill.cooldownTime * 1000));
        isCoolingDown = false;
    }

    // --- xử lý click chuột / touch ---
    public void OnPointerClick(PointerEventData eventData)
    {
        // left click / tap -> kích hoạt skill
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            TryActivate();
        }
    }


    private string FormatKeyCode(KeyCode key)
    {
        string keyString = key.ToString();

        // Nếu là Alpha1 → 1, Alpha2 → 2, v.v...
        if (key >= KeyCode.Alpha0 && key <= KeyCode.Alpha9)
        {
            return keyString.Replace("Alpha", "");
        }

        // Nếu là Keypad1 → 1
        if (key >= KeyCode.Keypad0 && key <= KeyCode.Keypad9)
        {
            return keyString.Replace("Keypad", "");
        }

        // Nếu là Space → "␣", Escape → "Esc", v.v (tuỳ chọn)
        if (key == KeyCode.Space) return "␣";
        if (key == KeyCode.Escape) return "Esc";

        return keyString;
    }

}*/