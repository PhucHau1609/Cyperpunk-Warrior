using UnityEngine;

public class LyraDialogueController : MonoBehaviour
{
    private FloatingFollower follower;

    private void Awake()
    {
        follower = GetComponent<FloatingFollower>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Dialog")) return;

        if (follower != null && !follower.IsReadyForDialogue) return;

        DialogueData dialogue = GetDialogueDataForZone(other);
        if (dialogue == null) return;

        // Gọi hiển thị thoại trực tiếp, không cần icon
        DialogueManager.Instance?.StartDialogue(dialogue, this.transform);
    }

    private DialogueData GetDialogueDataForZone(Collider2D zone)
    {
        DialogueDataComponent component = zone.GetComponent<DialogueDataComponent>();
        return component != null ? component.dialogueData : null;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Dialog"))
        {
            if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
            {
                DialogueManager.Instance.CloseDialogue();
            }
        }
    }
}
