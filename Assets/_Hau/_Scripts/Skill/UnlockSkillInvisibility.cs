using UnityEngine;

public class UnlockSkillInvisibility : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            ObserverManager.Instance.PostEvent(EventID.UnlockSkill_Invisibility, SkillID.Invisibility);
        }
    }
}
