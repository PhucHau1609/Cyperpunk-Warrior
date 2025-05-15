using UnityEngine;

public class NPC : MonoBehaviour
{
    public GameObject speechIcon;
    public DialogueData dialogueData;
    public Quest requiredQuest;

    private void Update()
    {
        if (requiredQuest == null || QuestManager.Instance.IsQuestCompleted(requiredQuest))
        {
            if (PlayerInRange())
                speechIcon.SetActive(true);
        }
        else
        {
            speechIcon.SetActive(false);
        }
    }

    private bool PlayerInRange()
    {
        // Viết đơn giản cho demo, có thể thay bằng trigger
        return Vector2.Distance(transform.position, GameObject.FindGameObjectWithTag("Player").transform.position) < 2f;
    }

    public void OnSpeechIconClicked()
    {
        DialogueManager.Instance.StartDialogue(dialogueData, this.transform);
        speechIcon.SetActive(false);
    }
}
