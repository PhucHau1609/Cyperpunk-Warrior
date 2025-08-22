//using System;

//public class TurnManager
//{
//    public Turn CurrentTurn { get; private set; }
//    public int CurrentFlipAttempts { get; private set; }
//    public event Action<Turn> OnTurnChanged;

//    public void StartPlayerTurn()
//    {
//        CurrentTurn = Turn.Player;
//        OnTurnChanged?.Invoke(CurrentTurn);
//        ResetFlipAttempts();
//    }

//    public void SwitchTurn()
//    {
//        CurrentTurn = (CurrentTurn == Turn.Player) ? Turn.NPC : Turn.Player;
//        OnTurnChanged?.Invoke(CurrentTurn);
//        ResetFlipAttempts();
//    }

//    public void ResetFlipAttempts() => CurrentFlipAttempts = 0;
//    public void IncrementFlipAttempts() => CurrentFlipAttempts++;
//    public void Reset()
//    {
//        CurrentTurn = Turn.Player;
//        CurrentFlipAttempts = 0;
//    }
//}
