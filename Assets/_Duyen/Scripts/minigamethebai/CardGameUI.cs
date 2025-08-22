//using System.Collections;
//using TMPro;
//using UnityEngine;
//using UnityEngine.UI;

//public class CardGameUI : MonoBehaviour
//{
//    [SerializeField] private TMP_Text playerTimerText;
//    [SerializeField] private TMP_Text npcTimerText;
//    [SerializeField] private TMP_Text turnText;

//    [SerializeField] private GameObject startPanel; // panel nút start

//    private CardsController controller;

//    public void Initialize(CardsController controller)
//    {
//        this.controller = controller;
//        controller.OnTimerUpdated += UpdateTimers;
//        controller.OnTurnChanged += UpdateTurn;
//        controller.OnGameEnded += ShowGameResult;

//        if (startPanel != null)
//            startPanel.SetActive(true);
//    }

//    public void OnClickStart()
//    {
//        if (controller != null)
//            controller.StartGame();

//        if (startPanel != null)
//            startPanel.SetActive(false);
//    }

//    private void UpdateTimers(float playerTime, float npcTime)
//    {
//        if (playerTimerText != null)
//            playerTimerText.text = $"EREN: {Mathf.CeilToInt(playerTime)}s";
//        if (npcTimerText != null)
//            npcTimerText.text = $"JACK: {Mathf.CeilToInt(npcTime)}s";
//    }

//    private void UpdateTurn(Turn turn)
//    {
//        if (turnText != null)
//        {
//            turnText.gameObject.SetActive(true);
//            turnText.text = turn == Turn.Player ? "EREN" : "JACK";
//        }

//        if (playerTimerText != null && npcTimerText != null)
//        {
//            if (turn == Turn.Player)
//            {
//                playerTimerText.color = HexToColor("00BBD4"); // xanh cho người chơi
//                npcTimerText.color = HexToColor("2B474B");    // xám cho NPC
//            }
//            else
//            {
//                npcTimerText.color = HexToColor("00BBD4");    // xanh cho NPC
//                playerTimerText.color = HexToColor("2B474B"); // xám cho người chơi
//            }
//        }

//        StartCoroutine(DelayTurnText(1.5f, turn));
//    }

//    private IEnumerator DelayTurnText(float delay, Turn turn)
//    {
//        yield return new WaitForSeconds(delay);

//        if (turnText != null)
//            turnText.gameObject.SetActive(false);

//        controller.EnablePlay();   // unlock chơi
//    }

//    private void ShowGameResult(bool playerWon)
//    {
//        Debug.Log("Game Over - Player Won: " + playerWon);
//        if (startPanel != null)
//            startPanel.SetActive(true);
//    }

//    private Color HexToColor(string hex)
//    {
//        Color color;
//        if (ColorUtility.TryParseHtmlString("#" + hex, out color))
//            return color;
//        return Color.white;
//    }

//}
