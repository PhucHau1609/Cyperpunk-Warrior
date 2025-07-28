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
    private GameState currentState = GameState.Gameplay;
    public GameState CurrentState => currentState;

    public bool IsGameplay => currentState == GameState.Gameplay;

    public void SetState(GameState newState)
    {
        Debug.Log($"[GameStateManager] SetState from {currentState} → {newState}", this);
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
}
