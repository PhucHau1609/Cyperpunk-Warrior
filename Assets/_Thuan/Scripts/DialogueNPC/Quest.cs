using UnityEngine;

[CreateAssetMenu(fileName = "Quest", menuName = "Quest/QuestData")]
public class Quest : ScriptableObject
{
    public string questName;
    public bool isCompleted;
}
