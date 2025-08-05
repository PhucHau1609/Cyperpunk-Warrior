using UnityEngine;
using UnityEngine.UI;

public class MiniGameOpener : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject miniGameUI;
    [SerializeField] private GameObject closeButtonUI;
    [SerializeField] private GameObject player;
    [SerializeField] private Button openMiniGameButton;

    [Header("Interaction Settings")]
    [SerializeField] private Transform interactionPoint;
    [SerializeField] private float interactionDistance = 3f;

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

    void Update()
    {
        if (player != null && interactionPoint != null && openMiniGameButton != null)
        {
            float distance = Vector3.Distance(player.transform.position, interactionPoint.position);
            openMiniGameButton.interactable = (distance <= interactionDistance);
        }
    }

    public void OpenMiniGame()
    {
        Debug.Log("OpenMiniGame() Called");

        // ⚠️ Chỉ cho mở nếu đã mở khóa Pet trong CodeLock
        if (!CodeLock.PetUnlocked)
        {
            Debug.Log("MiniGame chưa được mở khóa từ CodeLock!");
            return;
        }

        GameStateManager.Instance.SetState(GameState.MiniGame);

        float distance = Vector3.Distance(player.transform.position, interactionPoint.position);
        if (distance > interactionDistance) return;

        if (miniGameUI != null) miniGameUI.SetActive(true);
        if (closeButtonUI != null) closeButtonUI.SetActive(true);

        if (movementScript != null)
        {
            movementScript.enabled = false;
        }
    }


    public void CloseMiniGame()
    {
        if (miniGameUI != null) miniGameUI.SetActive(false);
        if (closeButtonUI != null) closeButtonUI.SetActive(false);

        if (movementScript != null)
        {
            movementScript.enabled = true;
        }

        GameStateManager.Instance.ResetToGameplay();

    }
}
