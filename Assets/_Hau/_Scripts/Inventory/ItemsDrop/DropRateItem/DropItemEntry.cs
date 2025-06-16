using System;
using UnityEngine;

[Serializable]
public class DropItemEntry
{
    public ItemCode itemCode;
    [Range(0f, 1f)] public float dropChance = 1f; // Xác suất rơi (0–1)
    public int minAmount = 1;
    public int maxAmount = 1;
}
