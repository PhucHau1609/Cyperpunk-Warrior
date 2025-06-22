using UnityEngine;

public class LyraDialogueController : MonoBehaviour
{
    public GameObject speechIconPrefab;
    private GameObject currentIconInstance;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Dialog")
        {
            DialogueData dialogue = GetDialogueDataForZone(other);
            if (dialogue == null) return;

            if (currentIconInstance != null)
            {
                Destroy(currentIconInstance);
            }

            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                currentIconInstance = Instantiate(speechIconPrefab, canvas.transform);
                var follower = currentIconInstance.GetComponent<SpeechIconFollower>();
                follower.targetNPC = this.transform;

                var clickHandler = currentIconInstance.GetComponent<SpeechIconClickHandler>();
                clickHandler.dialogueData = dialogue;
                clickHandler.npcTransform = this.transform;
            
            }
        }
    }

    private bool TagExists(string tag)
    {
        try
        {
            GameObject temp = new GameObject();
            temp.tag = tag;
            Destroy(temp);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Dialog"))
        {
            // Khi thoát khỏi vùng thoại -> xóa icon nếu đang hiện
            if (currentIconInstance != null)
            {
                Destroy(currentIconInstance);
                currentIconInstance = null;
            }
        }
    }

    DialogueData GetDialogueDataForZone(Collider2D zone)
    {
        DialogueDataComponent component = zone.GetComponent<DialogueDataComponent>();
        return component != null ? component.dialogueData : null;
    }
}
