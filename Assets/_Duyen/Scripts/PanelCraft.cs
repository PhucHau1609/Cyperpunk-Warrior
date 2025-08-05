using UnityEngine;

public class PanelCraft : HauSingleton<PanelCraft>
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
        Debug.Log("Crafting is Toggle");
        panel.SetActive(!panel.activeSelf);
    }
}
