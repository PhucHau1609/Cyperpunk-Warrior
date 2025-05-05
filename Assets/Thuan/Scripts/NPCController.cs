using UnityEngine;

public class NPCController : MonoBehaviour
{
    public Dialog dialog;
    private bool isPlayerNearby;

    private void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            var dialogManager = FindObjectOfType<DialogManager>();
            if (dialogManager.dialogState == DialogState.Free)
            {
                dialogManager.ShowInteractPanel(dialog);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
        }
    }
}
