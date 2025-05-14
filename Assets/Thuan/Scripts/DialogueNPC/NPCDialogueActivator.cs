using UnityEngine;

public class NPCDialogueActivator : MonoBehaviour
{
    public MonoBehaviour triggerSource; // Gán Trigger (PickupTrigger, JumpTrigger, v.v.)
    public GameObject speechIcon;
    public DialogueData dialogueData;

    private bool canTalk = false;

    private void Start()
    {
        if (triggerSource is ITriggerCondition condition)
        {
            condition.OnConditionMet += ActivateDialogue;
        }
        else
        {
            Debug.LogWarning("Trigger không implement ITriggerCondition!");
        }

        speechIcon.SetActive(false);
    }

    private void ActivateDialogue()
    {
        canTalk = true;
        speechIcon.SetActive(true);
    }

    public void TryInteract()
    {
        if (canTalk)
        {
            DialogueManager.Instance.StartDialogue(dialogueData, transform);
        }
    }
}
