using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class pausegame : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pannelpause;
    public GameObject panelOptions;
    public Button pauseButton;
    public Sprite imagePause; // Hình icon khi đang pause
    public Sprite imagePlay;  // Hình icon ban đầu

    private Image pauseButtonImage;

    void Start()
    {
        pauseButtonImage = pauseButton.GetComponent<Image>();
        if (pauseButtonImage != null && imagePlay != null)
        {
            pauseButtonImage.sprite = imagePlay;
        }

        if (pannelpause != null)
        {
            pannelpause.transform.localScale = Vector3.zero;
            GetCanvasGroup(pannelpause).alpha = 0f;
            pannelpause.SetActive(false);
        }

        if (panelOptions != null)
        {
            panelOptions.transform.localScale = Vector3.zero;
            GetCanvasGroup(panelOptions).alpha = 0f;
            panelOptions.SetActive(false);
        }
    }

    // Khi nhấn nút "Dừng"
    public void OnPauseClicked()
    {
        AudioManager.Instance.PlayClickSFX();

        if (pannelpause != null)
        {
            pannelpause.SetActive(true);
            pannelpause.transform.localScale = Vector3.zero;
            pannelpause.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack).SetUpdate(true);
            GetCanvasGroup(pannelpause).DOFade(1f, 0.25f).SetUpdate(true);            
        }
        Time.timeScale = 0f;

        // Đổi hình và vô hiệu hóa nút
        if (pauseButtonImage != null && imagePause != null)
        {
            pauseButtonImage.sprite = imagePause;
        }
        pauseButton.interactable = false;
    }

    // Khi nhấn "Tiếp tục" hoặc "Đóng"
    public void OnResumeClicked()
    {
        AudioManager.Instance.PlayClickSFX();

        Time.timeScale = 1f; // Tiếp tục game

        if (pannelpause != null)
        {
            var cg = GetCanvasGroup(pannelpause);
            pannelpause.transform.DOScale(0f, 0.2f).SetEase(Ease.InBack).OnComplete(() => {
                pannelpause.SetActive(false);
            });
            cg.DOFade(0f, 0.2f);
        }

        if (panelOptions != null)
        {
            var cg = GetCanvasGroup(panelOptions);
            panelOptions.transform.DOScale(0f, 0.2f).SetEase(Ease.InBack).OnComplete(() => {
                panelOptions.SetActive(false);
            });
            cg.DOFade(0f, 0.2f);
        }

        // Đổi lại hình và bật lại nút
        if (pauseButtonImage != null && imagePlay != null)
        {
            pauseButtonImage.sprite = imagePlay;
        }
        pauseButton.interactable = true;
    }

    // Khi nhấn "Quit"
    public void OnQuitClicked()
    {
        AudioManager.Instance.PlayClickSFX();
        Time.timeScale = 1f;
        SceneManager.LoadScene(0); // Về Scene chọn màn
    }

    // Khi nhấn "Chơi lại"
    public void OnOpenOptionsClicked()
    {
        AudioManager.Instance.PlayClickSFX();

        if (panelOptions != null)
        {
            var cg = GetCanvasGroup(panelOptions); // ✅ Lấy đúng CanvasGroup
            panelOptions.SetActive(true);
            panelOptions.transform.localScale = Vector3.zero;
            panelOptions.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack).SetUpdate(true);
            cg.DOFade(1f, 0.25f).SetUpdate(true);
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }

        if (pannelpause != null)
        {
            var cg = GetCanvasGroup(pannelpause);
            pannelpause.transform.DOScale(0f, 0.2f).SetEase(Ease.InBack).OnComplete(() => {
                pannelpause.SetActive(false);
            });
            cg.DOFade(0f, 0.2f).SetUpdate(true);
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }
    }

    public void OnRetryClicked(int sceneIndex)
    {
        AudioManager.Instance.PlayClickSFX();
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneIndex);
    }


    private void OnDisable()
    {
        // Reset timeScale để tránh game bị kẹt khi chuyển scene
        Time.timeScale = 1f;
    }
    private CanvasGroup GetCanvasGroup(GameObject go)
    {
        var cg = go.GetComponent<CanvasGroup>();
        if (cg == null) cg = go.AddComponent<CanvasGroup>();
        return cg;
    }
}
