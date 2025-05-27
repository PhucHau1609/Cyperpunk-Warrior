using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public GameObject speechIcon;
    public GameObject dialogueCanvas;

    private bool playerInRange = false;
    private bool hasTalked = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !hasTalked)
        {
            speechIcon.SetActive(true);
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            speechIcon.SetActive(false);
            playerInRange = false;
        }
    }

    public void OnSpeechIconClicked()
    {
        if (!hasTalked && playerInRange)
        {
            dialogueCanvas.SetActive(true);
            speechIcon.SetActive(false);  // Ẩn biểu tượng sau khi bấm
            hasTalked = true;             // Đánh dấu đã nói chuyện
        }
    }
}
