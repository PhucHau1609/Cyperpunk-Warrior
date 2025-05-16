using UnityEngine;

public class LyraDialogueController : MonoBehaviour
{
    public GameObject speechIconPrefab;
    private GameObject currentIconInstance;

    public DialogueData defaultDialogue;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Dialog"))
        {
            DialogueData dialogue = GetDialogueDataForZone(other);

            if (dialogue == null) dialogue = defaultDialogue;

            if (currentIconInstance == null)
            {
                currentIconInstance = Instantiate(speechIconPrefab, FindObjectOfType<Canvas>().transform);
                var follower = currentIconInstance.GetComponent<SpeechIconFollower>();
                follower.targetNPC = this.transform;

                var clickHandler = currentIconInstance.GetComponent<SpeechIconClickHandler>();
                clickHandler.dialogueData = dialogue;
                clickHandler.npcTransform = this.transform;
            }
            else
            {
                // Update nếu icon đang có
                var clickHandler = currentIconInstance.GetComponent<SpeechIconClickHandler>();
                clickHandler.dialogueData = dialogue;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Dialog"))
        {
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

//using UnityEngine;

//public class LyraDialogueController : MonoBehaviour
//{
//    public GameObject speechIconPrefab;
//    private GameObject currentIconInstance;

//    private bool inDialogZone = false;
//    private DialogueData currentDialogue;

//    public DialogueData defaultDialogue;  // có thể để null hoặc câu mặc định

//    void Start()
//    {
//        currentIconInstance = null;
//    }

//    void OnTriggerEnter2D(Collider2D other)
//    {
//        if (other.CompareTag("Dialog"))
//        {
//            inDialogZone = true;
//            currentDialogue = GetDialogueDataForZone(other);

//            if (currentIconInstance == null)
//            {
//                currentIconInstance = Instantiate(speechIconPrefab, FindObjectOfType<Canvas>().transform);
//                var follower = currentIconInstance.GetComponent<SpeechIconFollower>();
//                follower.targetNPC = this.transform;

//                var clickHandler = currentIconInstance.GetComponent<SpeechIconClickHandler>();
//                clickHandler.dialogueData = currentDialogue;
//                clickHandler.npcTransform = this.transform;
//            }
//            else
//            {
//                // Update dialogue data nếu icon đã hiện rồi
//                var clickHandler = currentIconInstance.GetComponent<SpeechIconClickHandler>();
//                clickHandler.dialogueData = currentDialogue;
//            }
//        }
//    }

//    void OnTriggerExit2D(Collider2D other)
//    {
//        if (other.CompareTag("Dialog"))
//        {
//            inDialogZone = false;
//            currentDialogue = null;
//            if (currentIconInstance != null)
//            {
//                Destroy(currentIconInstance);
//                currentIconInstance = null;
//            }
//        }
//    }

//    DialogueData GetDialogueDataForZone(Collider2D zoneCollider)
//    {
//        // Cách lấy DialogueData cho từng vùng, ví dụ lấy từ một component trên vùng trigger
//        var zoneDialogue = zoneCollider.GetComponent<DialogueDataComponent>();
//        if (zoneDialogue != null)
//            return zoneDialogue.dialogueData;
//        else
//            return defaultDialogue;
//    }
//}

