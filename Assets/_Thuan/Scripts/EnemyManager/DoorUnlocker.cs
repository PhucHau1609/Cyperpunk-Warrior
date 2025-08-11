using UnityEngine;

public class DoorUnlocker : MonoBehaviour
{
    [Header("Door")]
    public Animator doorAnimator;

    [Header("Requirement")]
    public ItemCode requiredItem = ItemCode.Clothes_1;

    private bool allEnemiesDead = false;
    private bool hasRequiredItem = false;
    private bool opened = false;

    private void OnEnable()
    {
        EnemyDeathManager.AllEnemiesDead += OnAllEnemiesDead;
        ItemCollectionTracker.ItemCollected += OnItemCollected;
    }

    private void Start()
    {
        // Đồng bộ trạng thái nếu vào giữa chừng:
        allEnemiesDead = EnemyDeathManager.Instance != null && EnemyDeathManager.Instance.AreAllEnemiesDead;
        hasRequiredItem = ItemCollectionTracker.Instance != null &&
                          ItemCollectionTracker.Instance.CollectedClothes.Contains(requiredItem);
        TryOpen();
    }

    private void OnDisable()
    {
        EnemyDeathManager.AllEnemiesDead -= OnAllEnemiesDead;
        ItemCollectionTracker.ItemCollected -= OnItemCollected;
    }

    private void OnAllEnemiesDead()
    {
        allEnemiesDead = true;
        TryOpen();
    }

    private void OnItemCollected(ItemCode code)
    {
        if (code == requiredItem)
        {
            hasRequiredItem = true;
            TryOpen();
        }
    }

    private void TryOpen()
    {
        if (!opened && allEnemiesDead && hasRequiredItem)
        {
            opened = true;
            doorAnimator?.SetTrigger("Open");
        }
    }
}
