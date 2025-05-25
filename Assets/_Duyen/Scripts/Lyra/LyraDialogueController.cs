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
            if (dialogue == null)
                return; // Vùng này không có thoại, không hiện icon

            if (currentIconInstance == null)
            {
                currentIconInstance = Instantiate(speechIconPrefab, FindObjectOfType<Canvas>().transform);
                var follower = currentIconInstance.GetComponent<SpeechIconFollower>();
                follower.targetNPC = this.transform;

                var clickHandler = currentIconInstance.GetComponent<SpeechIconClickHandler>();
                clickHandler.dialogueData = dialogue;
                clickHandler.npcTransform = this.transform;
                Collider2D triggerZone = other;
                DialogueData usedDialogue = dialogue;

                //clickHandler.onClick = () =>
                //{
                //    DialogueManager.Instance.onDialogueEnd = () =>
                //    {
                //    };
                //};
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
        if (TagExists("Dialog") && other.CompareTag("Dialog"))
        {
            DialogueData dialogue = GetDialogueDataForZone(other);
            if (dialogue == null)
                return;

            if (currentIconInstance == null)
            {
                currentIconInstance = Instantiate(speechIconPrefab, FindObjectOfType<Canvas>().transform);
                var follower = currentIconInstance.GetComponent<SpeechIconFollower>();
                follower.targetNPC = this.transform;

                var clickHandler = currentIconInstance.GetComponent<SpeechIconClickHandler>();
                clickHandler.dialogueData = dialogue;
                clickHandler.npcTransform = this.transform;
            }
        }
    }

    DialogueData GetDialogueDataForZone(Collider2D zone)
    {
        DialogueDataComponent component = zone.GetComponent<DialogueDataComponent>();
        return component != null ? component.dialogueData : null;
    }
}
