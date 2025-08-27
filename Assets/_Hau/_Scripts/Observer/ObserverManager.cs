/*using System;
using System.Collections.Generic;
using UnityEngine;

public enum EventID
{
    None,
    PlayerDied,
    EnemyKilled,
    ScoreChanged,
    LevelCompleted,
    FirstItemPickedUp,
    GameStateChanged,

    // Thêm các sự kiện khác tùy theo game của bạn

    // 🔹 Các phím hotkey
    OpenInventory,
    InventoryChanged,

    //Weapon
    Weapon_Toggle,
    Weapon_Swap,

    //Skill UnLock
    UnlockSkill_Invisibility,
    UnlockSkill_ColorRamp,
    UnlockSkill_Swap,
    UnlockSkill_Dash,




}

public class ObserverManager : HauSingleton<ObserverManager>
{
    private Dictionary<EventID, Action<object>> _eventDictionary = new();

    /// <summary>
    /// Đăng ký lắng nghe sự kiện
    /// </summary>
    public void AddListener(EventID eventID, Action<object> listener)
    {
        if (_eventDictionary.ContainsKey(eventID))
        {
            _eventDictionary[eventID] += listener;
        }
        else
        {
            _eventDictionary[eventID] = listener;
        }
    }

    /// <summary>
    /// Hủy đăng ký lắng nghe sự kiện
    /// </summary>
    public void RemoveListener(EventID eventID, Action<object> listener)
    {
        if (_eventDictionary.ContainsKey(eventID))
        {
            _eventDictionary[eventID] -= listener;

            if (_eventDictionary[eventID] == null)
            {
                _eventDictionary.Remove(eventID);
            }
        }
    }

    /// <summary>
    /// Gửi sự kiện cho tất cả listener
    /// </summary>
    public void PostEvent(EventID eventID, object param = null)
    {
        if (_eventDictionary.TryGetValue(eventID, out var callback))
        {
            callback?.Invoke(param);
        }
    }

    /// <summary>
    /// Xóa toàn bộ sự kiện (nếu cần reset game)
    /// </summary>
    public void ClearAllListeners()
    {
        _eventDictionary.Clear();
    }
}
*/

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Danh sách các sự kiện có thể xảy ra trong game
/// </summary>
public enum EventID
{
    None,
    PlayerDied,
    EnemyKilled,
    ScoreChanged,
    LevelCompleted,
    FirstItemPickedUp,
    GameStateChanged,
    FirstGunPickedUp,
    EquipmentChanged,


    // UI & Input
    OpenInventory,
    InventoryChanged,

    // Weapon
    Weapon_Toggle,
    Weapon_Swap,

    // Skill Unlocks
    UnlockSkill_Invisibility,
    UnlockSkill_ColorRamp,
    UnlockSkill_Swap,
    UnlockSkill_Dash,


    FirstEnergyPickedUp,
    NotHasCraftingRecipeInInventory,
    SecondEnergyPickedUp,
    SkillPrereqChanged

}

/// <summary>
/// Hệ thống Observer (EventBus) toàn cục.
/// Cho phép các đối tượng đăng ký và nhận sự kiện.
/// </summary>
public class ObserverManager : HauSingleton<ObserverManager>
{
    private Dictionary<EventID, Action<object>> eventDictionary = new();

    /// <summary>
    /// Đăng ký một listener cho một sự kiện cụ thể
    /// </summary>
    public void AddListener(EventID eventID, Action<object> listener)
    {
        if (listener == null) return;

        if (eventDictionary.TryGetValue(eventID, out var existingDelegate))
        {
            eventDictionary[eventID] = existingDelegate + listener;
        }
        else
        {
            eventDictionary[eventID] = listener;
        }
    }

    /// <summary>
    /// Gỡ một listener khỏi một sự kiện
    /// </summary>
    public void RemoveListener(EventID eventID, Action<object> listener)
    {
        if (listener == null) return;

        if (eventDictionary.TryGetValue(eventID, out var existingDelegate))
        {
            existingDelegate -= listener;

            if (existingDelegate == null)
                eventDictionary.Remove(eventID);
            else
                eventDictionary[eventID] = existingDelegate;
        }
    }

    /// <summary>
    /// Gửi sự kiện tới tất cả listener đã đăng ký
    /// </summary>
    public void PostEvent(EventID eventID, object param = null)
    {
        if (eventDictionary.TryGetValue(eventID, out var callback))
        {
            callback?.Invoke(param);
        }
    }

    /// <summary>
    /// Xóa toàn bộ listener (dùng khi reset game)
    /// </summary>
    public void ClearAllListeners()
    {
        eventDictionary.Clear();
    }
}
