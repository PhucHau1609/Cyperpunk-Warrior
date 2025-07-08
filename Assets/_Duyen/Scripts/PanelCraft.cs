using UnityEngine;

public class PanelCraft : MonoBehaviour
{
    public GameObject panel;  // Kéo panel cần mở vào đây trong Inspector

    public void OpenPanel()
    {
        panel.SetActive(true);
    }

    public void ClosePanel()
    {
        panel.SetActive(false);
    }

    public void TogglePanel()
    {
        panel.SetActive(!panel.activeSelf);
    }
}
