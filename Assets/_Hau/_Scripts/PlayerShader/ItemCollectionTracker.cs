using System.Collections.Generic;
using UnityEngine;

public class ItemCollectionTracker : MonoBehaviour
{
    public static ItemCollectionTracker Instance;

    private HashSet<ItemCode> collectedClothes = new HashSet<ItemCode>();
    private bool hasCollectedArtefact = false;
    private bool conditionMet = false;

    public bool ConditionMet => conditionMet;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void OnItemCollected(ItemCode itemCode)
    {
        bool isRelevantItem = false;

        if (itemCode.ToString().StartsWith("Clothes"))
        {
            isRelevantItem = collectedClothes.Add(itemCode);
        }
        else if (itemCode.ToString().StartsWith("Artefacts"))
        {
            if (!hasCollectedArtefact)
            {
                hasCollectedArtefact = true;
                isRelevantItem = true;
            }
        }

        // Chỉ kiểm tra nếu là item liên quan
        if (isRelevantItem)
        {
            CheckCompletion();
        }
    }

    private void CheckCompletion()
    {
        if (!conditionMet && collectedClothes.Count >= 4 && hasCollectedArtefact)
        {
            conditionMet = true;
            Debug.Log("✅ Đã nhặt đủ điều kiện biến hình! Nhấn phím [1] để kích hoạt.");
        }
    }
}

