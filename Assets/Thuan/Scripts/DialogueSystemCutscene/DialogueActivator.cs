using UnityEngine;

public class DialogueActivator : MonoBehaviour
{
    public GameObject dialogueHolder;

    public void ActivateDialogue()
    {
        dialogueHolder.SetActive(true);
    }
}
