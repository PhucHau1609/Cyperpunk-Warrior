using UnityEngine;

public class NPCDialogueUnlockManager : MonoBehaviour
{
    public GameObject speechIcon;
    public DialogueData dialogueData;

    private NPCState currentState = NPCState.Locked;

    private void Start()
    {
        speechIcon.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Dialog"))
        {
            currentState = NPCState.CanTalk;
            speechIcon.SetActive(true);
        }
    }

    //private void OnDestroy()
    //{
    //    EnemyDialogue.OnAnyEnemyKilled -= OnEnemyKilled;
    //    ExtraConditionTrigger.OnExtraConditionMet -= OnExtraCondition;
    //}

    //void OnEnemyKilled()
    //{
    //    currentState = NPCState.CanTalk;
    //    speechIcon.SetActive(true);
    //}

    //void OnExtraCondition()
    //{
    //    currentState = NPCState.CanTalk;
    //    speechIcon.SetActive(true);
    //}

    public void OnSpeechIconClicked()
    {
        if (currentState == NPCState.CanTalk)
        {
            //DialogueManager.Instance.StartDialogue(dialogueData, transform);
            currentState = NPCState.Waiting;
            speechIcon.SetActive(false);
        }
    }
}
