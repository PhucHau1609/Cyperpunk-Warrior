using UnityEngine;

public class CanvasTrigger2D : MonoBehaviour
{
    public CanvasGroup targetCanvasGroup; // Kéo CanvasGroup của canvas vào
    private bool playerInside = false;

    void Start()
    {
        if (targetCanvasGroup != null)
            targetCanvasGroup.interactable = false; // Ban đầu không bấm được
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInside = true;
            if (targetCanvasGroup != null)
                targetCanvasGroup.interactable = true; // Cho phép bấm
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInside = false;
            if (targetCanvasGroup != null)
                targetCanvasGroup.interactable = false; // Khoá bấm
        }
    }
}
