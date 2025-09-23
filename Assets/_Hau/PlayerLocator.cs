using System;
using UnityEngine;

public class PlayerLocator : MonoBehaviour
{
    public static Transform Current { get; private set; }
    public static event Action<Transform> OnChanged;

    public static void Set(Transform player)
    {
        if (Current == player) return;
        Current = player;
        OnChanged?.Invoke(Current);
    }
}
