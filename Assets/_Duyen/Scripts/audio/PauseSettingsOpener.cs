using UnityEngine;
using UnityEngine.UI;

public class PauseSettingsOpener : MonoBehaviour
{
    [SerializeField] private Button settingsButton;

    void Start()
    {
        settingsButton.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlayClickSFX();
            AudioSettingsUI.Instance.OpenSettingsPanel();
        });
    }
}
