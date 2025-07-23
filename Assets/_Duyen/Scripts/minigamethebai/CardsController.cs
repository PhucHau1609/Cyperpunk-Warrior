using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class CardsController : MonoBehaviour
{
    [Header("Prefabs & Transforms")]
    [SerializeField] Card cardPrefab;
    [SerializeField] Transform gridTransform;
    [SerializeField] Transform playerHandTransform;
    [SerializeField] Transform playedCardSlot;
    [SerializeField] Transform deckPileTransform;
    [SerializeField] Transform npcHandTransform;

    [Header("Sprites")]
    [SerializeField] Sprite[] sprites;
    [SerializeField] Sprite hiddenCardSprite;

    [Header("UI Panels")]
    [SerializeField] GameObject gamePanel;
    [SerializeField] GameObject finishPanel;
    [SerializeField] GameObject buttonPanel;
    [SerializeField] GameObject banPanel;
    [SerializeField] GameObject barrierObject;
    [SerializeField] TMPro.TextMeshProUGUI playerTimerText;
    [SerializeField] TMPro.TextMeshProUGUI npcTimerText;

    private List<Card> deckCards = new List<Card>();
    private List<Card> playerHand = new List<Card>();
    private List<Card> npcHand = new List<Card>();
    private Card playedCard;

    private float playerTimeRemaining = 60f;
    private float npcTimeRemaining = 60f;
    private bool isTimerRunning = false;

    private List<Card> flippedCards = new List<Card>();

    [SerializeField] Transform cameraFocusPoint;
    private Transform playerTransform;

    private enum Turn { Player, NPC }
    private Turn currentTurn = Turn.Player;

    private int playerFlipAttempts = 0;
    private int npcFlipAttempts = 0;
    private bool isPlayerInputEnabled = true;


    void Start()
    {
        gamePanel.SetActive(false);
        finishPanel.SetActive(false);
        buttonPanel.SetActive(true);
        banPanel.SetActive(true);
        playerTransform = GameObject.FindWithTag("Player")?.transform;
    }

    public void StartGame()
    {
        gamePanel.SetActive(true);
        finishPanel.SetActive(false);
        buttonPanel.SetActive(false);
        banPanel.SetActive(true);

        CameraFollow.Instance.Target = cameraFocusPoint;

        playerTimeRemaining = 60f;
        npcTimeRemaining = 60f;
        isTimerRunning = false;

        ResetGame();

        Invoke(nameof(StartTimerAfterDeal), 2f);
    }

    void StartTimerAfterDeal()
    {
        isTimerRunning = true;
    }

    void ResetGame()
    {
        foreach (Transform t in gridTransform) Destroy(t.gameObject);
        foreach (Transform t in playerHandTransform) Destroy(t.gameObject);
        if (playedCard != null) Destroy(playedCard.gameObject);

        deckCards.Clear();
        playerHand.Clear();
        flippedCards.Clear();
        playedCard = null;

        playerFlipAttempts = 0;
        npcFlipAttempts = 0;
        currentTurn = Turn.Player;

        List<Sprite> deckSprites = new List<Sprite>();
        for (int i = 0; i < sprites.Length; i++) deckSprites.Add(sprites[i]);
        Shuffle(deckSprites);

        StartCoroutine(FillDeckCards(deckSprites));
    }

    void Update()
    {
        if (!isTimerRunning) return;

        float delta = Time.deltaTime;

        if (currentTurn == Turn.Player)
        {
            playerTimeRemaining -= delta;
            if (playerTimeRemaining <= 0)
            {
                playerTimeRemaining = 0;
                if (playerHand.Count > 0) GameOverPanel.Instance.ShowGameOver();  // Player thua
            }
        }
        else if (currentTurn == Turn.NPC)
        {
            npcTimeRemaining -= delta;
            if (npcTimeRemaining <= 0)
            {
                npcTimeRemaining = 0;
                if (npcHand.Count > 0) EndGame(true); // NPC thua
            }
        }

        UpdateTimerUI();
    }

    public void OnCardClicked(Card card)
    {
        if (!isPlayerInputEnabled || currentTurn != Turn.Player) return;

        if (playerHand.Contains(card) && playedCard == null)
        {
            playedCard = card;
            playerHand.Remove(card);
            card.transform.SetParent(playedCardSlot);
            card.transform.DOLocalMove(Vector3.zero, 0.3f).SetEase(Ease.OutQuad);
            card.transform.DOLocalRotate(Vector3.zero, 0.3f).SetEase(Ease.OutQuad);
            card.transform.DOScale(Vector3.one * 1.2f, 0.3f).SetEase(Ease.OutQuad);
            AudioManager.Instance?.PlayPlayCard();
            ArrangeHand();
        }
        else if (deckCards.Contains(card) && playedCard != null && playerFlipAttempts < 5 && !flippedCards.Contains(card))
        {
            card.Show();
            AudioManager.Instance?.PlayFlipCard();
            flippedCards.Add(card);
            playerFlipAttempts++;

            if (card.iconSprite == playedCard.iconSprite)
            {
                StartCoroutine(HandleMatched(card, true));
            }
            else if (playerFlipAttempts >= 5)
            {
                StartCoroutine(HandleMismatch(true));
            }
        }
    }

    Card GetNPCPlayedCard()
    {
        if (npcHand.Count == 0) return null;

        Card npcCard = npcHand[0];
        npcHand.RemoveAt(0);

        npcCard.gameObject.SetActive(true);
        npcCard.transform.SetParent(playedCardSlot);
        npcCard.transform.localPosition = Vector3.zero;
        npcCard.transform.localRotation = Quaternion.identity;
        npcCard.transform.localScale = Vector3.one * 1.2f;

        npcCard.iconImage.sprite = npcCard.iconSprite;

        AudioManager.Instance?.PlayPlayCard();
        Debug.Log($"NPC đánh lá: {npcCard.iconSprite.name}, còn {npcHand.Count} lá");

        playedCard = npcCard;
        return npcCard;
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
            AudioManager.Instance?.PlayDealCard();
            yield return new WaitForSeconds(0.12f);
        }

        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(FillPlayerHand(deckSprites));
        yield return StartCoroutine(FillNPCHand(deckSprites));
    }

    IEnumerator FillNPCHand(List<Sprite> deckSprites)
    {
        for (int i = 0; i < 4; i++)
        {
            Sprite sp = deckSprites[Random.Range(0, deckSprites.Count)];

            Card c = Instantiate(cardPrefab, deckPileTransform);
            c.SetIconSprite(sp, false); // cho dễ debug
            c.hiddenIconSprite = hiddenCardSprite;
            c.controller = this;

            // Không set parent => không hiển thị
            c.transform.SetParent(npcHandTransform, false);
            c.iconImage.sprite = hiddenCardSprite;
            //c.gameObject.SetActive(false); // Ẩn hẳn đi nếu muốn

            npcHand.Add(c);
        }

        Debug.Log($"NPC có {npcHand.Count} lá bài.");
        yield return null;
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
            ArrangeHand();
            Vector3 toPos = c.transform.position;

            c.transform.position = fromPos;
            c.transform.localScale = Vector3.one * 0.2f;

            c.transform.DOMove(toPos, 0.4f).SetEase(Ease.OutQuad);
            c.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);

            playerHand.Add(c);
            AudioManager.Instance?.PlayDealCard();
            yield return new WaitForSeconds(0.15f);
        }

        ArrangeHand();
        //StartCoroutine(FillNPCHand(deckSprites));

    }

    IEnumerator HandleMatched(Card matchCard, bool isPlayer)
    {
        yield return new WaitForSeconds(0.2f);
        AudioManager.Instance?.PlayCorrect();

        Card tempCard = playedCard;
        if (tempCard != null)
        {
            CanvasGroup cg = tempCard.GetComponent<CanvasGroup>() ?? tempCard.gameObject.AddComponent<CanvasGroup>();

            Sequence seq1 = DOTween.Sequence();
            seq1.Append(tempCard.transform.DOShakeRotation(0.25f, new Vector3(0, 0, 5f), 10, 45f));
            seq1.Append(cg.DOFade(0, 0.3f));
            seq1.OnComplete(() => Destroy(tempCard.gameObject));
        }

        if (matchCard != null)
        {
            Sequence seq2 = DOTween.Sequence();
            seq2.Append(matchCard.transform.DOScale(new Vector3(1.05f, 1.1f, 1f), 0.2f).SetEase(Ease.OutBack));
            seq2.Append(matchCard.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.InOutSine));
        }

        yield return new WaitForSeconds(0.5f);

        foreach (Card c in flippedCards) c?.Hide();
        flippedCards.Clear();
        playedCard = null;

        if (isPlayer)
        {
            playerFlipAttempts = 0;
            AddCardToOpponent(); // Player thắng thì NPC bị cộng thêm
        }
        else
        {
            npcFlipAttempts = 0;
            AddRandomCardToHand(); // NPC thắng thì Player bị cộng thêm
        }

        CheckWinLose();

        if (!isPlayer)
        {
            StartCoroutine(NPCTurn()); // NPC tiếp tục lật
        }
    }

    IEnumerator HandleMismatch(bool isPlayer)
    {
        yield return new WaitForSeconds(0.5f);
        AudioManager.Instance?.PlayWrong();

        foreach (Card c in flippedCards)
        {
            Sequence seq = DOTween.Sequence();
            seq.Append(c.transform.DOShakePosition(0.25f, new Vector3(0.15f, 0f, 0f), 8, 90, false));
            seq.AppendCallback(() => c.Hide());
        }

        yield return new WaitForSeconds(0.4f);

        if (playedCard != null)
        {
            if (isPlayer)
            {
                playedCard.transform.SetParent(playerHandTransform);
                playedCard.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutQuad);
                playedCard.iconImage.sprite = playedCard.iconSprite;
                playerHand.Add(playedCard);
                AudioManager.Instance?.PlayReturnCard();

                currentTurn = Turn.NPC;
                isPlayerInputEnabled = false;
                StartCoroutine(NPCTurn());
            }
            else
            {
                playedCard.transform.SetParent(npcHandTransform);
                Canvas.ForceUpdateCanvases();
                ArrangeHand(); // Để lấy vị trí đúng của lá bài trong tay NPC (nếu có)
                Vector3 toPos = playedCard.transform.position;

                // Di chuyển từ chỗ đánh về vị trí mới
                playedCard.transform.localScale = Vector3.one * 1.2f;
                playedCard.transform.DOScale(1f, 0.3f).SetEase(Ease.OutQuad);
                playedCard.transform.DOMove(toPos, 0.3f).SetEase(Ease.OutQuad);
                playedCard.transform.DOLocalRotate(Vector3.zero, 0.3f).SetEase(Ease.OutQuad);

                // Ẩn icon lại nếu cần
                playedCard.iconImage.sprite = hiddenCardSprite;

                npcHand.Add(playedCard);
                AudioManager.Instance?.PlayReturnCard();

                currentTurn = Turn.Player;
                isPlayerInputEnabled = true;
                playerFlipAttempts = 0;
            }
        }

        playedCard = null;
        flippedCards.Clear();

        //if (isPlayer)
        //{
        //    playerFlipAttempts = 0;
        //    //AddRandomCardToHand();
        //    currentTurn = Turn.NPC;
        //    isPlayerInputEnabled = false;
        //    StartCoroutine(NPCTurn());
        //}
        //else
        //{
        //    npcFlipAttempts = 0;
        //    currentTurn = Turn.Player;
        //    isPlayerInputEnabled = true;

        //}


        ArrangeHand();
        CheckWinLose();
    }

    IEnumerator NPCTurn()
    {
        yield return new WaitForSeconds(1f);

        Card npcPlayedCard = GetNPCPlayedCard();
        if (npcPlayedCard == null)
        {
            currentTurn = Turn.Player;
            isPlayerInputEnabled = true;
            yield break;
        }

        npcFlipAttempts = 0;
        //bool foundMatch = false;

        while (npcFlipAttempts < 5)
        {
            Card randomCard = GetRandomUnflippedCard();
            if (randomCard == null) break;

            randomCard.Show();
            AudioManager.Instance?.PlayFlipCard();
            flippedCards.Add(randomCard);
            npcFlipAttempts++;

            yield return new WaitForSeconds(0.6f);

            if (randomCard.iconSprite == npcPlayedCard.iconSprite)
            {
                //foundMatch = true;
                yield return StartCoroutine(HandleMatched(randomCard, false));
                yield break;
            }
        }

        yield return StartCoroutine(HandleMismatch(false));
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
        AudioManager.Instance?.PlayDealCard();
        ArrangeHand();
    }

    void AddCardToOpponent()
    {
        Sprite sp = sprites[Random.Range(0, sprites.Length)];
        Card c = Instantiate(cardPrefab, npcHandTransform);
        c.SetIconSprite(sp, false);
        c.hiddenIconSprite = hiddenCardSprite;
        c.controller = this;
        c.gameObject.SetActive(true); // ẩn

        npcHand.Add(c);
        Debug.Log($"NPC bị cộng thêm 1 lá. Hiện có {npcHand.Count} lá.");
    }

    Card GetRandomUnflippedCard()
    {
        List<Card> available = deckCards.FindAll(c => !flippedCards.Contains(c));
        if (available.Count == 0) return null;
        return available[Random.Range(0, available.Count)];
    }

    void ArrangeHand()
    {
        int count = playerHand.Count;
        if (count == 0) return;

        float spacing = 1f;
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
        if (playerHand.Count == 0 && npcHand.Count > 0) //player thang
        {
            EndGame(true);
            //GameOverPanel.Instance.ShowGameOver();
        }
        else if (npcHand.Count == 0 && playerHand.Count > 0) // player thua
        {
            GameOverPanel.Instance.ShowGameOver(); //EndGame(true); 
        }
    }

    void UpdateTimerUI()
    {
        playerTimerText.text = Mathf.CeilToInt(playerTimeRemaining).ToString() + "s";
        npcTimerText.text = Mathf.CeilToInt(npcTimeRemaining).ToString() + "s";
    }

    void EndGame(bool playerWin)
    {
        isTimerRunning = false;
        gamePanel.SetActive(false);
        finishPanel.SetActive(true);
        buttonPanel.SetActive(playerWin);
        banPanel.SetActive(!playerWin);
        CameraFollow.Instance.Target = playerTransform;

        if (playerWin)
        {
            AudioManager.Instance?.PlayWinGame();
            if (barrierObject != null) barrierObject.SetActive(false);
        }
        else
        {
            AudioManager.Instance?.PlayLoseGame();
        }
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}