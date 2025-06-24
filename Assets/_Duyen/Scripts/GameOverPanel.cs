using UnityEngine;

public class GameOverPanel : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;

    public static GameOverPanel Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
        else
            Debug.LogWarning("GameOverPanel chưa được gán trong Inspector.");
    }
}
