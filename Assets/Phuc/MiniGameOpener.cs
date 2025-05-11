using UnityEngine;

public class MiniGameOpener : MonoBehaviour
{
    [SerializeField] private GameObject miniGameUI;  // UI của mini game
    [SerializeField] private GameObject closeButtonUI; // UI của button tắt game

    void Start()
    {
        // Đảm bảo rằng cả 2 UI đều bị ẩn khi bắt đầu
        if (miniGameUI != null)
        {
            miniGameUI.SetActive(false); // Ẩn mini game khi bắt đầu
        }

        if (closeButtonUI != null)
        {
            closeButtonUI.SetActive(false); // Ẩn button tắt game khi bắt đầu
        }
    }

    // Hàm mở mini game khi bấm nút Start
    public void OpenMiniGame()
    {
        if (miniGameUI != null)
        {
            miniGameUI.SetActive(true); // Mở mini game
        }

        if (closeButtonUI != null)
        {
            closeButtonUI.SetActive(true); // Hiển thị nút tắt mini game
        }
    }

    // Hàm tắt mini game khi bấm nút Close
    public void CloseMiniGame()
    {
        if (miniGameUI != null)
        {
            miniGameUI.SetActive(false); // Ẩn mini game
        }

        if (closeButtonUI != null)
        {
            closeButtonUI.SetActive(false); // Ẩn nút tắt game
        }
    }
}
