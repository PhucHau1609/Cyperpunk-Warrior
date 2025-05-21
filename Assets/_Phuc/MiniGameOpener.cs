using UnityEngine;

public class MiniGameOpener : MonoBehaviour
{
    [SerializeField] private GameObject miniGameUI;
    [SerializeField] private GameObject closeButtonUI;
    [SerializeField] private GameObject player;

    private PlayerMovement movementScript;

    void Start()
    {
        if (miniGameUI != null) miniGameUI.SetActive(false);
        if (closeButtonUI != null) closeButtonUI.SetActive(false);

        if (player != null)
        {
            movementScript = player.GetComponent<PlayerMovement>();
        }
    }

    public void OpenMiniGame()
    {
        if (miniGameUI != null) miniGameUI.SetActive(true);
        if (closeButtonUI != null) closeButtonUI.SetActive(true);

        // Tắt điều khiển Player
        if (movementScript != null)
        {
            movementScript.enabled = false;
        }
    }

    public void CloseMiniGame()
    {
        if (miniGameUI != null) miniGameUI.SetActive(false);
        if (closeButtonUI != null) closeButtonUI.SetActive(false);

        // Bật lại điều khiển Player
        if (movementScript != null)
        {
            movementScript.enabled = true;
        }
    }
}
