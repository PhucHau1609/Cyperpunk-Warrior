using System;
using System.Collections.Generic;

public static class QuestEventBus
{
    // Sự kiện đếm số lần (ví dụ: "code_collected", "enemy_killed"...)
    private static readonly Dictionary<string, Action<int>> _eventCount = new();

    public static void Raise(string eventName, int amount = 1)
    {
        if (string.IsNullOrEmpty(eventName)) return;
        if (_eventCount.TryGetValue(eventName, out var cb))
            cb?.Invoke(amount);
    }

    public static void SubscribeCount(string eventName, Action<int> callback)
    {
        if (string.IsNullOrEmpty(eventName) || callback == null) return;
        if (_eventCount.TryGetValue(eventName, out var ex)) _eventCount[eventName] = ex + callback;
        else _eventCount[eventName] = callback;
    }

    public static void UnsubscribeCount(string eventName, Action<int> callback)
    {
        if (string.IsNullOrEmpty(eventName) || callback == null) return;
        if (_eventCount.TryGetValue(eventName, out var ex)) _eventCount[eventName] = ex - callback;
    }
}
