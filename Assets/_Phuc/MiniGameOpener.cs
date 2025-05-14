using UnityEngine;

public class MiniGameOpener : MonoBehaviour
{
    [SerializeField] private GameObject miniGameUI;      // UI của mini game (GameObject, không phải Canvas)
    [SerializeField] private GameObject closeButtonUI;   // Nút tắt mini game
    [SerializeField] private GameObject player;          // Player GameObject

    void Start()
    {
        if (miniGameUI != null) miniGameUI.SetActive(false);
        if (closeButtonUI != null) closeButtonUI.SetActive(false);
    }

    public void OpenMiniGame()
    {
        if (miniGameUI != null) miniGameUI.SetActive(true);
        if (closeButtonUI != null) closeButtonUI.SetActive(true);

        // 🔒 Tắt điều khiển của Player
        if (player != null)
        {
            PlayerMovement movementScript = player.GetComponent<PlayerMovement>();
            if (movementScript != null)
            {
                movementScript.enabled = false;
            }
        }
    }

    public void CloseMiniGame()
    {
        if (miniGameUI != null) miniGameUI.SetActive(false);
        if (closeButtonUI != null) closeButtonUI.SetActive(false);

        // ✅ Bật lại điều khiển của Player
        if (player != null)
        {
            PlayerMovement movementScript = player.GetComponent<PlayerMovement>();
            if (movementScript != null)
            {
                movementScript.enabled = true;
            }
        }
    }
}
