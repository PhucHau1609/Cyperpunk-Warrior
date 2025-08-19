using UnityEngine;
public enum GameState
{
    Gameplay,
    Inventory,
    Crafting,
    MiniGame,
    Dialogue,
    Paused,
    Cutscene,
}

public class GameStateManager : HauSingleton<GameStateManager>
{
    [SerializeField] protected GameState currentState = GameState.Gameplay;
    public GameState CurrentState => currentState;

    public bool IsGameplay => currentState == GameState.Gameplay;

    public void SetState(GameState newState)
    {
        //Debug.Log($"[GameStateManager] SetState from {currentState} → {newState}", this);
        currentState = newState;
        ObserverManager.Instance?.PostEvent(EventID.GameStateChanged, currentState);
    }


    public void ResetToGameplay()
    {
        SetState(GameState.Gameplay);
    }

    public bool IsState(GameState state)
    {
        return currentState == state;
    }

    public bool IsBusy()
    {
        return currentState != GameState.Gameplay;
    }

    public GameState GetCurrentState()
    {
        return currentState;
    }

    // Method để debug current state
    public void DebugCurrentState()
    {
        Debug.Log($"[GameStateManager] === GAME STATE DEBUG ===");
        Debug.Log($"Current State: {currentState}");
        Debug.Log($"Is Gameplay: {IsGameplay}");
        Debug.Log($"Is Busy: {IsBusy()}");
        Debug.Log($"[GameStateManager] === END DEBUG ===");
    }

    // Method để force reset về Gameplay (bypass mọi checks)
    public void ForceResetToGameplay()
    {
        Debug.Log($"[GameStateManager] FORCE RESET: {currentState} → Gameplay");
        currentState = GameState.Gameplay;
        ObserverManager.Instance?.PostEvent(EventID.GameStateChanged, currentState);
    }

    // Method để manual set state (for debugging)
    public void ForceSetState(GameState newState)
    {
        Debug.Log($"[GameStateManager] FORCE SET: {currentState} → {newState}");
        currentState = newState;
        ObserverManager.Instance?.PostEvent(EventID.GameStateChanged, currentState);
}
}
