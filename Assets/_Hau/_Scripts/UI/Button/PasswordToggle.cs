using UnityEngine;
using UnityEngine.UI;

public class PasswordToggleUI : MonoBehaviour
{
    [Header("References")]
    public InputField passwordInput;
    public Button toggleButton;
    public Image toggleIcon;

    [Header("Icons")]
    public Sprite showIcon;  // Icon con mắt mở
    public Sprite hideIcon;  // Icon con mắt gạch ngang

    private bool isPasswordHidden = true;

    void Start()
    {
        if (toggleButton != null)
            toggleButton.onClick.AddListener(TogglePassword);
    }

    void TogglePassword()
    {
        isPasswordHidden = !isPasswordHidden;

        if (isPasswordHidden)
        {
            passwordInput.contentType = InputField.ContentType.Password;
            if (toggleIcon) toggleIcon.sprite = hideIcon;
        }
        else
        {
            passwordInput.contentType = InputField.ContentType.Standard;
            if (toggleIcon) toggleIcon.sprite = showIcon;
        }

        // bắt buộc refresh lại text để áp dụng kiểu mới
        passwordInput.ForceLabelUpdate();
    }
}
