using UnityEngine;

public class UnlockSkillManager : MonoBehaviour
{
    [SerializeField] protected EventID eventID;
    [SerializeField] protected SkillID skillID;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            ObserverManager.Instance.PostEvent(eventID, skillID);
        }
    }
}
