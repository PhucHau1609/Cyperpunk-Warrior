using UnityEngine;

public class NPCDialogueTrigger : MonoBehaviour
{
    //public GameObject speechIconPrefab;       // Prefab icon thoại
    //public Transform npcTransform;            // NPC liên quan
    //public DialogueData dialogueData;         // Dữ liệu thoại
    //private GameObject currentIconInstance;

    //void OnTriggerEnter2D(Collider2D other)
    //{
    //    if (other.CompareTag("Player") && currentIconInstance == null)
    //    {
    //        // Tạo icon trên đầu NPC
    //        currentIconInstance = Instantiate(speechIconPrefab, FindObjectOfType<Canvas>().transform);

    //        var follower = currentIconInstance.GetComponent<SpeechIconFollower>();
    //        follower.targetNPC = npcTransform;

    //        var clickHandler = currentIconInstance.GetComponent<SpeechIconClickHandler>();
    //        clickHandler.dialogueData = dialogueData;
    //        clickHandler.npcTransform = npcTransform;
    //    }
    //}

    //void OnTriggerExit2D(Collider2D other)
    //{
    //    if (other.CompareTag("Player") && currentIconInstance != null)
    //    {
    //        Destroy(currentIconInstance);
    //    }
    //}
}
