using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PauseSettingsOpener : MonoBehaviour
{
    [SerializeField] private Button settingsButton;
    [SerializeField] private GameObject panelOptions;

    private void Start()
    {
        if (panelOptions == null)
        {
            panelOptions = GameObject.Find("PanelOptions");
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        }
    }

    private void OnSettingsButtonClicked()
    {
        AudioManager.Instance?.PlayClickSFX();

        if (panelOptions != null && panelOptions.activeSelf)
        {
            CanvasGroup cg = panelOptions.GetComponent<CanvasGroup>();
            if (cg == null) cg = panelOptions.AddComponent<CanvasGroup>();

            cg.DOFade(0f, 0.2f).SetUpdate(true);
            panelOptions.transform.DOScale(0.8f, 0.2f).SetUpdate(true).OnComplete(() =>
            {
                panelOptions.SetActive(false);
                AudioSettingsUI.Instance?.OpenSettingsPanel();
            });
        }
        else
        {
            AudioSettingsUI.Instance?.OpenSettingsPanel();
        }
    }
}
