using UnityEngine;
using System.Collections.Generic;

public class ItemDropTable : MonoBehaviour
{
    [SerializeField] private List<DropItemEntry> dropEntries = new();
    [SerializeField] private float spawnHeight = 1f;
    //[SerializeField] private float forceAmount = 5f;

    public void TryDropItems()
    {
        Vector3 dropPosition = transform.position;

        foreach (var entry in dropEntries)
        {
            float roll = Random.value;
            if (roll <= entry.dropChance)
            {
                int amount = Random.Range(entry.minAmount, entry.maxAmount + 1);

                ItemsDropManager.Instance.DropManyItems(
                    entry.itemCode,
                    amount,
                    dropPosition + new Vector3(0, spawnHeight, 0)
                );
            }
        }
    }
}
