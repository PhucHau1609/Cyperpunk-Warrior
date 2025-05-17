using UnityEngine;
using System.Collections.Generic;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;
    private void Awake() => Instance = this;

    private HashSet<Quest> completedQuests = new HashSet<Quest>();

    public void CompleteQuest(Quest quest)
    {
        if (!completedQuests.Contains(quest))
            completedQuests.Add(quest);
    }

    public bool IsQuestCompleted(Quest quest)
    {
        return completedQuests.Contains(quest);
    }
}
