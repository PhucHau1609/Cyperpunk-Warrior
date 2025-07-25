using UnityEngine;

public class UnlockSkillISwap : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            ObserverManager.Instance.PostEvent(EventID.UnlockSkill_Swap, SkillID.Swap);
        }
    }
}
