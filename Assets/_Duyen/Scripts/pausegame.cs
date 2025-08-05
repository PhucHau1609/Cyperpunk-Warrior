using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class pausegame : MonoBehaviour
{
    [Header("UI References")]
    public GameObject btnPause;
    public GameObject pannelpause;
    public GameObject panelOptions;
    public Button pauseButton;
    public Sprite imagePause;
    public Sprite imagePlay;

    private Image pauseButtonImage;
    public PlayerMovement playerMovement; // ⚠️ GÁN TRONG INSPECTOR


    public static pausegame Instance { get; private set; }
    public static bool IsPaused { get; private set; } = false;


    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        pauseButtonImage = pauseButton.GetComponent<Image>();
        if (pauseButtonImage != null && imagePlay != null)
        {
            pauseButtonImage.sprite = imagePlay;
        }

        // Chuẩn bị trạng thái animation cho các panel
        InitPanel(pannelpause);
        InitPanel(panelOptions);
    }

    private void InitPanel(GameObject panel)
    {
        if (panel != null)
        {
            panel.SetActive(false);
            var canvasGroup = panel.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = panel.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
            panel.transform.localScale = Vector3.one * 0.8f;
        }
    }

    public void OnPauseClicked()
    {
        // 🔒 CHẶN nếu không phải đang ở Gameplay
        if (!GameStateManager.Instance.IsGameplay)
        {
            Debug.Log("Không thể pause khi đang ở trạng thái: " + GameStateManager.Instance.CurrentState);
            return;
        }

        GameStateManager.Instance.SetState(GameState.Paused);
        AudioManager.Instance.PlayClickSFX();
        if (playerMovement != null)
            playerMovement.SetCanMove(false);

        Time.timeScale = 0f;
        pannelpause.SetActive(true);

        var pauseCanvasGroup = pannelpause.GetComponent<CanvasGroup>();
        if (pauseCanvasGroup == null) pauseCanvasGroup = pannelpause.AddComponent<CanvasGroup>();

        pannelpause.transform.localScale = Vector3.one * 0.8f;
        pauseCanvasGroup.alpha = 0;

        // Hiệu ứng xuất hiện
        pannelpause.transform.DOScale(1f, 0.3f)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);

        pauseCanvasGroup.DOFade(1f, 0.3f)
            .SetUpdate(true);

        if (pauseButtonImage != null && imagePause != null)
        {
            pauseButtonImage.sprite = imagePause;
        }

        IsPaused = true;
        pauseButton.interactable = false;
    }

    public void OnResumeClicked()
    {
        GameStateManager.Instance.ResetToGameplay();
        AudioManager.Instance.PlayClickSFX();

        var pauseCanvasGroup = pannelpause.GetComponent<CanvasGroup>();
        if (pauseCanvasGroup == null) pauseCanvasGroup = pannelpause.AddComponent<CanvasGroup>();

        pauseCanvasGroup.DOFade(0f, 0.2f).SetUpdate(true);
        pannelpause.transform.DOScale(0.8f, 0.2f).SetUpdate(true);

        if (panelOptions.activeSelf)
        {
            var optionsCanvasGroup = panelOptions.GetComponent<CanvasGroup>();
            if (optionsCanvasGroup == null) optionsCanvasGroup = panelOptions.AddComponent<CanvasGroup>();

            optionsCanvasGroup.DOFade(0f, 0.2f).SetUpdate(true);
            panelOptions.transform.DOScale(0.8f, 0.2f).SetUpdate(true);
        }

        DOVirtual.DelayedCall(0.22f, () =>
        {
            Time.timeScale = 1f;

            if (playerMovement != null)
                playerMovement.SetCanMove(true);

            pannelpause.SetActive(false);
            panelOptions.SetActive(false);

            if (pauseButtonImage != null && imagePlay != null)
            {
                pauseButtonImage.sprite = imagePlay;
            }

            pauseButton.interactable = true;
            IsPaused = false;
        }, ignoreTimeScale: true);
    }

    public void OnQuitClicked()
    {
        AudioManager.Instance.PlayClickSFX();
        Time.timeScale = 1f;
        SceneManager.LoadScene(0); // Trở về menu chính
    }

    public void OnOpenOptionsClicked()
    {
        AudioManager.Instance.PlayClickSFX();
        StartCoroutine(ShowOptionsPanelAfterDelay());
    }

    private IEnumerator ShowOptionsPanelAfterDelay()
    {
        if (pannelpause != null)
        {
            var pauseCanvasGroup = pannelpause.GetComponent<CanvasGroup>();
            if (pauseCanvasGroup == null) pauseCanvasGroup = pannelpause.AddComponent<CanvasGroup>();

            pauseCanvasGroup.DOFade(0f, 0.2f).SetUpdate(true);
            pannelpause.transform.DOScale(0.8f, 0.2f).SetUpdate(true);

            yield return new WaitForSecondsRealtime(0.25f);
            pannelpause.SetActive(false);
        }

        if (panelOptions != null)
        {
            panelOptions.SetActive(true);
            var optionsCanvasGroup = panelOptions.GetComponent<CanvasGroup>();
            if (optionsCanvasGroup == null) optionsCanvasGroup = panelOptions.AddComponent<CanvasGroup>();

            panelOptions.transform.localScale = Vector3.one * 0.8f;
            optionsCanvasGroup.alpha = 0;

            panelOptions.transform.DOScale(1f, 0.3f)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);

            optionsCanvasGroup.DOFade(1f, 0.3f)
                .SetUpdate(true);
        }
    }

    public void OnRetryClicked(int sceneIndex)
    {
        AudioManager.Instance.PlayClickSFX();
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneIndex);
    }

    public void CheckIsOpenInventory()
    {
        if (GameStateManager.Instance.CurrentState == GameState.Inventory)
        {
            Debug.Log("Dang mo inventory");
            return;
        }
    }

    public void ToggleBTNPause()
    {
        Debug.Log("1");
        btnPause.SetActive(!btnPause.activeSelf);
    }    

    // Nếu bạn muốn đảm bảo rằng game sẽ không bị đóng băng khi object bị vô hiệu hóa, bạn có thể bật lại dòng này:
    // private void OnDisable() => Time.timeScale = 1f;
}
