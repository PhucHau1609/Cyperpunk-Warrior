using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class pausegame : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pannelpause;
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
    }

    // Khi nhấn nút "Dừng"
    public void OnPauseClicked()
    {
        AudioManager.Instance.PlayClickSFX();

        Time.timeScale = 0f; // Dừng thời gian
        pannelpause.SetActive(true);

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
        pannelpause.SetActive(false);

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
    public void OnRetryClicked(int sceneIndex)
    {
        AudioManager.Instance.PlayClickSFX();
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneIndex); // Load lại màn chơi được chỉ định
    }

    private void OnDisable()
    {
        // Reset timeScale để tránh game bị kẹt khi chuyển scene
        Time.timeScale = 1f;
    }
}
