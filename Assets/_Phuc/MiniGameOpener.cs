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
    [Tooltip("Ảnh nền trắng đặt phía sau button để làm nổi bật khi ở trong tầm tương tác.")]
    [SerializeField] private GameObject highlightImage; // ảnh nền trắng cho button (đặt inactive lúc đầu)

    [Header("Interaction Settings")]
    [SerializeField] private Transform interactionPoint;
    [SerializeField] private float interactionDistance = 3f;

    // >>> Thêm panel hướng dẫn <<<
    [Header("Guide Panel (thêm mới)")]
    [SerializeField] private GameObject guidePanel;

    private PlayerMovement movementScript;

    private void Start()
    {
        if (miniGameUI != null) miniGameUI.SetActive(false);
        if (closeButtonUI != null) closeButtonUI.SetActive(false);
        if (highlightImage != null) highlightImage.SetActive(false);

        // >>> Thêm: đảm bảo panel hướng dẫn tắt lúc đầu
        if (guidePanel != null) guidePanel.SetActive(false);

        if (player != null)
        {
            movementScript = player.GetComponent<PlayerMovement>();
        }
    }

    private void Update()
    {
        if (player == null || interactionPoint == null || openMiniGameButton == null) return;

        float distance = Vector3.Distance(player.transform.position, interactionPoint.position);
        bool canInteract = (distance <= interactionDistance);

        // Bật/tắt khả năng bấm của button
        openMiniGameButton.interactable = canInteract;

        // Hiện/ẩn nền trắng để làm nổi bật khi vào tầm
        if (highlightImage != null)
            highlightImage.SetActive(canInteract);
    }

    public void OpenMiniGame()
    {
        // ⚠️ Chỉ cho mở nếu đã mở khóa Pet trong CodeLock
        if (!CodeLock.PetUnlocked)
        {
            // Debug.Log("MiniGame chưa được mở khóa từ CodeLock!");
            return;
        }

        // Kiểm tra có CraftingRecipe trong inventory không
        if (!InventoryManager.Instance.HasItemInInventory(ItemCode.CraftingRecipe))
        {
            // Debug.Log("Không thể mở MiniGame — thiếu CraftingRecipe trong inventory.");
            ObserverManager.Instance.PostEvent(EventID.NotHasCraftingRecipeInInventory);
            return;
        }

        // Kiểm tra lại khoảng cách an toàn
        if (player == null || interactionPoint == null) return;
        float distance = Vector3.Distance(player.transform.position, interactionPoint.position);
        if (distance > interactionDistance) return;

        // Chuyển state vào MiniGame
        GameStateManager.Instance.SetState(GameState.MiniGame);

        // Bật UI mini game
        if (miniGameUI != null) miniGameUI.SetActive(true);
        if (closeButtonUI != null) closeButtonUI.SetActive(true);

        // >>> Thêm: bật panel hướng dẫn cùng lúc mở mini game
        if (guidePanel != null) guidePanel.SetActive(true);

        // Khóa di chuyển
        if (movementScript != null)
            movementScript.enabled = false;
    }

    public void CloseMiniGame()
    {
        // Tắt UI mini game
        if (miniGameUI != null) miniGameUI.SetActive(false);
        if (closeButtonUI != null) closeButtonUI.SetActive(false);

        // >>> Thêm: tắt panel hướng dẫn khi đóng mini game
        if (guidePanel != null) guidePanel.SetActive(false);

        // Mở lại di chuyển
        if (movementScript != null)
            movementScript.enabled = true;

        // Trả state về gameplay
        GameStateManager.Instance.ResetToGameplay();

        // Khi đóng mini game, ẩn highlight (phòng trường hợp player vẫn đứng gần)
        if (highlightImage != null)
            highlightImage.SetActive(false);
    }
}
