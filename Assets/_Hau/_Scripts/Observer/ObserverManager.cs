using System;
using System.Collections.Generic;
using UnityEngine;

public enum EventID
{
    None,
    PlayerDied,
    EnemyKilled,
    ScoreChanged,
    LevelCompleted,
    // Thêm các sự kiện khác tùy theo game của bạn

    // 🔹 Các phím hotkey
    OpenInventory,
    InventoryChanged,

    //Weapon
    Weapon_Toggle,
    Weapon_Swap,


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
