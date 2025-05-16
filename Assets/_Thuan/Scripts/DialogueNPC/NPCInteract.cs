using UnityEngine;

public class NPCInteract : MonoBehaviour
{
    private NPCUnlockOnEnemyKilled npcUnlock;

    void Start()
    {
        npcUnlock = GetComponent<NPCUnlockOnEnemyKilled>();
    }

    void OnMouseDown() // hoặc dùng raycast nếu thích
    {
        npcUnlock.TryInteract();
    }
}
