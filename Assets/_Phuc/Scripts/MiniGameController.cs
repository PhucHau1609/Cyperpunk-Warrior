using UnityEngine;

public class MiniGameController : MonoBehaviour
{
    public GameObject miniGameUI;  // Đối tượng UI của mini game
    public GameObject closeButton; // Đối tượng Button "X"

    // Hàm để tắt mini game và ẩn nút "X"
    public void CloseMiniGame()
    {
        miniGameUI.SetActive(false);  // Tắt UI của mini game
        closeButton.SetActive(false); // Ẩn nút "X"
    }
}
