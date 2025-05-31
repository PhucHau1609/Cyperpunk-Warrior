using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public GameObject speechIcon;
    public GameObject dialogueCanvas; 
    public Vector3 iconOffset = new Vector3(0, 1.5f, 0); 

    private bool playerInRange = false;
    private bool hasTalked = false;
    private Transform playerTransform;

    void Start()
    {
        if (speechIcon != null)
            speechIcon.SetActive(false);

        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(false);
    }

    void Update()
    {
        // Di chuyển icon theo đầu Player
        if (playerInRange && playerTransform != null && speechIcon != null)
        {
            speechIcon.transform.position = playerTransform.position + iconOffset;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !hasTalked)
        {
            playerTransform = collision.transform;
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
            playerTransform = null;
        }
    }

    public void OnSpeechIconClicked()
    {
        if (playerInRange && !hasTalked)
        {
            if (dialogueCanvas != null)
                dialogueCanvas.SetActive(true);

            speechIcon.SetActive(false);
            hasTalked = true;
        }
    }
}
