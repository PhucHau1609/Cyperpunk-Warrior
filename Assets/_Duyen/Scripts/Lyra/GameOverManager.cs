using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    public GameObject gameOverPanel;
    public LyraHealth lyra;

    public void Retry()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (lyra != null)
            lyra.ResetLyra();
    }
}
