using UnityEngine;
using UnityEngine.UI;

public class MiniGameOpener : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject miniGameUI;
    [SerializeField] private GameObject closeButtonUI;
    [SerializeField] private GameObject player;
    [SerializeField] private Button openMiniGameButton;

    [Header("Button Highlight")]
    [SerializeField] private GameObject highlightImage; // ảnh nền trắng cho button

    [Header("Interaction Settings")]
    [SerializeField] private Transform interactionPoint;
    [SerializeField] private float interactionDistance = 3f;

    private PlayerMovement movementScript;

    void Start()
    {
        if (miniGameUI != null) miniGameUI.SetActive(false);
        if (closeButtonUI != null) closeButtonUI.SetActive(false);
        if (highlightImage != null) highlightImage.SetActive(false);

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
            bool canInteract = (distance <= interactionDistance);

            openMiniGameButton.interactable = canInteract;

            if (highlightImage != null)
                highlightImage.SetActive(canInteract);
        }
    }

    // ... phần còn lại giữ nguyên
}

