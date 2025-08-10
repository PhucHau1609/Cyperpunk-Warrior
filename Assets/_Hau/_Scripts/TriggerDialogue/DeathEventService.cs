using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathEventService : HauSingleton<DeathEventService>
{
    private readonly Dictionary<string, int> deathsByScene = new();

    protected override void Awake()
    {
        base.Awake();
        // Lắng nghe sự kiện chết một lần duy nhất, mọi scene đều dùng được
        ObserverManager.Instance.AddListener(EventID.PlayerDied, OnPlayerDied);
    }

    private void OnDestroy()
    {
        if (ObserverManager.Instance != null)
            ObserverManager.Instance.RemoveListener(EventID.PlayerDied, OnPlayerDied);
    }

    private void OnPlayerDied(object _)
    {
        var scene = SceneManager.GetActiveScene().name;
        deathsByScene[scene] = GetDeaths(scene) + 1;
        // Không gọi thoại ở đây nữa!
        // Chỉ ghi nhận số lần chết để nơi khác (trigger) kiểm tra điều kiện.
    }

    public int GetDeaths(string scene)
        => deathsByScene.TryGetValue(scene, out var c) ? c : 0;

    public int GetCurrentSceneDeaths()
        => GetDeaths(SceneManager.GetActiveScene().name);
}
