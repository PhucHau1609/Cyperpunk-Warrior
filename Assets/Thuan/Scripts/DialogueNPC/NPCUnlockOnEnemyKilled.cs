using UnityEngine;

public class NPCUnlockOnEnemyKilled : MonoBehaviour
{
    public GameObject speechIcon;
    public DialogueData dialogueData;

    private bool canTalk = false;

    private void Start()
    {
        speechIcon.SetActive(false);
        EnemyDialogue.OnAnyEnemyKilled += UnlockDialogue;
    }

    private void OnDestroy()
    {
        EnemyDialogue.OnAnyEnemyKilled -= UnlockDialogue;
    }

    void UnlockDialogue()
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
