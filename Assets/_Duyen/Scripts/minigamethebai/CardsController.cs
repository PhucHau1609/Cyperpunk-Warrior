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
    [SerializeField] Transform deckPileTransform;

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
        foreach (Transform t in gridTransform) Destroy(t.gameObject);
        foreach (Transform t in playerHandTransform) Destroy(t.gameObject);
        if (playedCard != null) Destroy(playedCard.gameObject);

        deckCards.Clear();
        playerHand.Clear();
        flippedCards.Clear();
        flipAttempts = 0;

        List<Sprite> deckSprites = new List<Sprite>();
        for (int i = 0; i < sprites.Length; i++) deckSprites.Add(sprites[i]);
        Shuffle(deckSprites);

        StartCoroutine(FillDeckCards(deckSprites));
    }

    public void OnCardClicked(Card card)
    {
        if (playerHand.Contains(card) && playedCard == null)
        {
            playedCard = card;
            playerHand.Remove(card);
            card.transform.SetParent(playedCardSlot);
            card.transform.DOLocalMove(Vector3.zero, 0.3f).SetEase(Ease.OutQuad);
            card.transform.DOLocalRotate(Vector3.zero, 0.3f).SetEase(Ease.OutQuad);
            card.transform.DOScale(Vector3.one * 1.5f, 0.3f).SetEase(Ease.OutQuad);

            ArrangeHand();
        }
        else if (deckCards.Contains(card) && playedCard != null && flipAttempts < 3 && !flippedCards.Contains(card))
        {
            card.Show();
            flippedCards.Add(card);
            flipAttempts++;

            if (card.iconSprite == playedCard.iconSprite)
            {
                StartCoroutine(HandleMatched(card));
            }
            else if (flipAttempts >= 3)
            {
                StartCoroutine(HandleMismatch());
            }
        }
    }
    
    IEnumerator FillDeckCards(List<Sprite> deckSprites)
    {
        for (int i = 0; i < 16 && i < deckSprites.Count; i++)
        {
            Sprite sp = deckSprites[i];
            Card c = Instantiate(cardPrefab, deckPileTransform);
            c.SetIconSprite(sp, false);
            c.hiddenIconSprite = hiddenCardSprite;
            c.controller = this;

            Vector3 fromPosition = c.transform.position;

            c.transform.SetParent(gridTransform);
            c.transform.SetSiblingIndex(i);

            Canvas.ForceUpdateCanvases();
            Vector3 toPosition = c.transform.position;

            c.transform.position = fromPosition;
            c.transform.localScale = Vector3.one * 0.2f;
            c.transform.DOMove(toPosition, 0.4f).SetEase(Ease.OutQuad);
            c.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);

            deckCards.Add(c);
            yield return new WaitForSeconds(0.12f);
        }

        yield return new WaitForSeconds(0.2f);
        StartCoroutine(FillPlayerHand(deckSprites));
    }

    IEnumerator FillPlayerHand(List<Sprite> deckSprites)
    {
        for (int i = 0; i < 4; i++)
        {
            Sprite sp = deckSprites[Random.Range(0, deckSprites.Count)];

            Card c = Instantiate(cardPrefab, deckPileTransform);
            c.SetIconSprite(sp, true);
            c.hiddenIconSprite = hiddenCardSprite;
            c.controller = this;

            Vector3 fromPos = c.transform.position;

            c.transform.SetParent(playerHandTransform);
            Canvas.ForceUpdateCanvases();
            ArrangeHand(); // cần gọi để layout xong mới lấy đúng vị trí
            Vector3 toPos = c.transform.position;

            c.transform.position = fromPos;
            c.transform.localScale = Vector3.one * 0.2f;

            c.transform.DOMove(toPos, 0.4f).SetEase(Ease.OutQuad);
            c.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);

            playerHand.Add(c);
            yield return new WaitForSeconds(0.15f);
        }

        ArrangeHand();
    }

    IEnumerator HandleMatched(Card matchCard)
    {
        yield return new WaitForSeconds(0.2f);

        // Lắc và mờ dần lá bài đánh ra
        Sequence seq1 = DOTween.Sequence();
        seq1.Append(playedCard.transform.DOShakeRotation(0.3f, 15, 10));
        seq1.Append(playedCard.GetComponent<CanvasGroup>().DOFade(0, 0.3f));
        seq1.OnComplete(() => Destroy(playedCard.gameObject));

        // Hiệu ứng matchCard: phóng to rồi thu nhỏ
        Sequence seq2 = DOTween.Sequence();
        seq2.Append(matchCard.transform.DOScale(1.5f, 0.2f).SetEase(Ease.OutBack));
        seq2.Append(matchCard.transform.DOScale(1f, 0.2f).SetEase(Ease.InOutSine));

        yield return new WaitForSeconds(0.5f);

        // Úp lại tất cả lá lật
        foreach (Card c in flippedCards)
        {
            if (c != null) c.Hide();
        }

        flippedCards.Clear();
        playedCard = null;
        flipAttempts = 0;

        CheckWinLose();
    }

    IEnumerator HandleMismatch()
    {
        yield return new WaitForSeconds(0.5f);

        foreach (Card c in flippedCards)
        {
            // Lắc nhẹ rồi ẩn
            Sequence seq = DOTween.Sequence();
            seq.Append(c.transform.DOShakePosition(0.3f, 10, 10));
            seq.AppendCallback(() => c.Hide());
        }

        yield return new WaitForSeconds(0.4f);

        // Trả lá bài về tay
        playedCard.transform.SetParent(playerHandTransform);
        playedCard.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutQuad);
        playedCard.iconImage.sprite = playedCard.iconSprite;
        playerHand.Add(playedCard);
        playedCard = null;

        AddRandomCardToHand();

        flippedCards.Clear();
        flipAttempts = 0;

        ArrangeHand();
        CheckWinLose();
    }

    void AddRandomCardToHand()
    {
        Sprite sp = sprites[Random.Range(0, sprites.Length)];
        Card newCard = Instantiate(cardPrefab, deckPileTransform);
        newCard.SetIconSprite(sp, true);
        newCard.hiddenIconSprite = hiddenCardSprite;
        newCard.controller = this;

        newCard.transform.localPosition = Vector3.zero;
        newCard.transform.SetParent(playerHandTransform);
        newCard.transform.DOLocalMove(Vector3.zero, 0.3f).SetEase(Ease.OutQuad);

        playerHand.Add(newCard);
        ArrangeHand();
    }

    void ArrangeHand()
    {
        int count = playerHand.Count;
        if (count == 0) return;

        float spacing = 80f;
        float totalWidth = spacing * (count - 1);
        float startX = -totalWidth / 2f;
        float y = 0f;

        for (int i = 0; i < count; i++)
        {
            float x = startX + spacing * i;
            int mid = count / 2;
            int offset = i - mid;
            float rotZ = offset * -3f;
            if (count % 2 == 0 && i == mid - 1) rotZ = 0;

            Vector3 pos = new Vector3(x, y, 0f);
            Vector3 rot = new Vector3(0f, 0f, rotZ);

            var card = playerHand[i];
            card.transform.DOLocalMove(pos, 0.3f).SetEase(Ease.OutQuad);
            card.transform.DOLocalRotate(rot, 0.3f);
            card.transform.SetSiblingIndex(i);
        }
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
