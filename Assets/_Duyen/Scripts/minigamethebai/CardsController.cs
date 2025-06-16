using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardsController : MonoBehaviour
{
    [Header("Prefabs & Transforms")]
    [SerializeField] Card cardPrefab;
    [SerializeField] Transform gridTransform;
    [SerializeField] Transform playerHandTransform;
    [SerializeField] Transform playedCardSlot;

    [Header("Sprites")]
    [SerializeField] Sprite[] sprites;
    [SerializeField] Sprite hiddenCardSprite;

    [Header("UI Panels")]
    [SerializeField] GameObject gamePanel;
    [SerializeField] GameObject finishPanel;

    private List<Card> deckCards = new List<Card>();
    private List<Card> playerHand = new List<Card>();
    private Card playedCard;

    private int flipAttempts = 0;
    private List<Card> flippedCards = new List<Card>();

    void Start()
    {
        gamePanel.SetActive(false);
        finishPanel.SetActive(false);
    }

    public void StartGame()
    {
        gamePanel.SetActive(true);
        finishPanel.SetActive(false);

        ResetGame();
    }

    void ResetGame()
    {
        // Clear existing
        foreach (Transform t in gridTransform) Destroy(t.gameObject);
        foreach (Transform t in playerHandTransform) Destroy(t.gameObject);
        if (playedCard != null) Destroy(playedCard.gameObject);

        deckCards.Clear();
        playerHand.Clear();
        flippedCards.Clear();
        flipAttempts = 0;

        // Build deck
        List<Sprite> deckSprites = new List<Sprite>();
        for (int i = 0; i < sprites.Length; i++) deckSprites.Add(sprites[i]);
        Shuffle(deckSprites);

        // Create deck cards
        for (int i = 0; i < 16 && i < deckSprites.Count; i++)
        {
            Card c = Instantiate(cardPrefab, gridTransform);
            c.SetIconSprite(deckSprites[i], false); // ẩn
            c.hiddenIconSprite = hiddenCardSprite;
            c.controller = this;
            deckCards.Add(c);
        }

        // Create 5 player cards
        for (int i = 0; i < 5; i++)
        {
            Card c = Instantiate(cardPrefab, playerHandTransform);
            c.SetIconSprite(deckSprites[Random.Range(0, deckSprites.Count)], true); // hiện
            c.hiddenIconSprite = hiddenCardSprite;
            c.controller = this;
            playerHand.Add(c);
        }
    }

    public void OnCardClicked(Card card)
    {
        if (playerHand.Contains(card) && playedCard == null)
        {
            // Đánh bài
            playedCard = card;
            playerHand.Remove(card);
            card.transform.SetParent(playedCardSlot);
            card.transform.localPosition = Vector3.zero;
        }
        else if (deckCards.Contains(card) && playedCard != null && flipAttempts < 3 && !flippedCards.Contains(card))
        {
            card.Show();
            flippedCards.Add(card);
            flipAttempts++;

            if (card.iconSprite == playedCard.iconSprite)
            {
                // Trùng -> destroy và reset lượt
                StartCoroutine(HandleMatched(card));
            }
            else if (flipAttempts >= 3)
            {
                StartCoroutine(HandleMismatch());
            }
        }
    }

    IEnumerator HandleMatched(Card matchCard)
    {
        yield return new WaitForSeconds(0.5f);

        playedCard.FadeOut();
        Destroy(playedCard.gameObject);
        playedCard = null;

        foreach (Card c in flippedCards)
        {
            c.Hide();
        }

        ShuffleDeck();
        flippedCards.Clear();
        flipAttempts = 0;

        CheckWinLose();
    }

    IEnumerator HandleMismatch()
    {
        yield return new WaitForSeconds(0.5f);

        foreach (Card c in flippedCards)
        {
            c.Hide();
        }

        ShuffleDeck();

        // Trả bài lại tay và thêm 1 bài mới
        playedCard.transform.SetParent(playerHandTransform);
        playedCard.iconImage.sprite = playedCard.iconSprite;
        playerHand.Add(playedCard);
        playedCard = null;

        AddRandomCardToHand();

        flippedCards.Clear();
        flipAttempts = 0;

        CheckWinLose();
    }

    void AddRandomCardToHand()
    {
        Sprite sp = sprites[Random.Range(0, sprites.Length)];
        Card newCard = Instantiate(cardPrefab, playerHandTransform);
        newCard.SetIconSprite(sp, true); // hiện mặt
        newCard.hiddenIconSprite = hiddenCardSprite;
        newCard.controller = this;
        playerHand.Add(newCard);
    }

    void CheckWinLose()
    {
        if (playerHand.Count == 0)
        {
            Debug.Log("WIN");
            gamePanel.SetActive(false);
            finishPanel.SetActive(true);
        }
        else if (playerHand.Count > 10)
        {
            Debug.Log("LOSE");
            gamePanel.SetActive(false);
            finishPanel.SetActive(true);
        }
    }

    void ShuffleDeck()
    {
        List<Sprite> currentSprites = new List<Sprite>();
        foreach (Card c in deckCards)
        {
            currentSprites.Add(c.iconSprite);
        }

        Shuffle(currentSprites);

        for (int i = 0; i < deckCards.Count; i++)
        {
            deckCards[i].SetIconSprite(currentSprites[i]);
        }
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
}