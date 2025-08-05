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

}