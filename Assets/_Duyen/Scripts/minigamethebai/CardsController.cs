/*using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// Controller chính cho mini-game đánh bài memory matching
/// Refactor theo chuẩn Senior Developer: SOLID principles, tách biệt trách nhiệm, quản lý state chắc chắn
/// </summary>
public class CardsController : MonoBehaviour
{
    #region Configuration - Cấu hình game
    [Header("Luật chơi")]
    [SerializeField] private GameConfig gameConfig = new GameConfig(); // Cài đặt các thông số game như số bài, thời gian, etc.

    [Header("Prefabs & Transforms - Các đối tượng và vị trí")]
    [SerializeField] private Card cardPrefab; // Template để tạo ra các lá bài
    [SerializeField] private Transform gridTransform; // Vùng chứa 12 lá bài trên bàn
    [SerializeField] private Transform playerHandTransform; // Vùng chứa bài trên tay Player
    [SerializeField] private Transform playedCardSlot; // Vị trí đặt lá bài vừa đánh ra
    [SerializeField] private Transform deckPileTransform; // Chồng bài gốc (để animation)
    [SerializeField] private Transform npcHandTransform; // Vùng chứa bài trên tay NPC

    [Header("Sprites & UI - Hình ảnh và giao diện")]
    [SerializeField] private Sprite[] cardSprites; // Mảng các hình ảnh lá bài
    [SerializeField] private Sprite hiddenCardSprite; // Hình mặt sau của lá bài (khi úp)
    [SerializeField] private CardGameUI gameUI; // Controller quản lý UI

    [Header("Tích hợp với game chính")]
    [SerializeField] private Transform cameraFocusPoint; // Điểm camera nhìn vào khi chơi mini-game
    #endregion

    #region Game State - Trạng thái game
    private CardGameStateMachine stateMachine; // Máy trạng thái: Menu -> Initializing -> Playing -> Finished
    private CardDeck deck; // Quản lý 12 lá bài trên bàn
    private PlayerHand playerHand; // Quản lý bài trên tay Player (3 lá)
    private PlayerHand npcHand; // Quản lý bài trên tay NPC (3 lá)
    private GameTimer gameTimer; // Hệ thống đếm thời gian (60s mỗi người)
    private TurnManager turnManager; // Quản lý lượt chơi (Player/NPC) và số lần lật bài
    private Card currentPlayedCard; // Lá bài hiện tại được đánh ra

    // Events để tách biệt các component (loose coupling)
    public event Action<bool> OnGameEnded; // Event khi game kết thúc (true = Player thắng)
    public event Action<Turn> OnTurnChanged; // Event khi đổi lượt
    public event Action<float, float> OnTimerUpdated; // Event cập nhật timer UI

    private bool canPlay = true;
    public bool CanPlay => canPlay;  // expose cho UI hoặc Card check

    public GameObject buttonpanel;
    #endregion

    public void EnablePlay()
    {
        canPlay = true;
    }

    #region Initialization - Khởi tạo
    void Awake()
    {
        InitializeComponents();
    }

    /// <summary>
    /// Khởi tạo tất cả các component và đăng ký events
    /// </summary>
    private void InitializeComponents()
    {
        // Khởi tạo các hệ thống core
        stateMachine = new CardGameStateMachine(); // Quản lý trạng thái game
        deck = new CardDeck(cardSprites, hiddenCardSprite, gameConfig); // Bộ bài trên bàn
        playerHand = new PlayerHand(gameConfig.maxHandSize, true); // Tay bài Player (hiện)
        npcHand = new PlayerHand(gameConfig.maxHandSize, false); // Tay bài NPC (ẩn)
        gameTimer = new GameTimer(gameConfig.turnTimeLimit); // Timer 60s
        turnManager = new TurnManager(); // Quản lý lượt chơi

        // Đăng ký các events (Observer pattern)
        gameTimer.OnTimeUp += HandleTimeUp; // Khi hết thời gian
        turnManager.OnTurnChanged += HandleTurnChanged; // Khi đổi lượt
        stateMachine.OnStateChanged += HandleStateChanged; // Khi đổi state game

        // Khởi tạo UI
        if (gameUI != null)
        {
            gameUI.Initialize(this); // UI tự đăng ký events từ controller
        }
    }

    /// <summary>
    /// Update loop chính - chỉ chạy khi game đang Playing
    /// </summary>
    void Update()
    {
        if (stateMachine.CurrentState == GameStates.Playing)
        {
            // Cập nhật timer và gửi event cho UI
            gameTimer.UpdateTimer(Time.deltaTime, turnManager.CurrentTurn);
            OnTimerUpdated?.Invoke(gameTimer.PlayerTimeRemaining, gameTimer.NPCTimeRemaining);
        }
    }
    #endregion

    #region Public Interface - Interface công khai
    /// <summary>
    /// Bắt đầu game - được gọi từ UI button
    /// </summary>
    public void StartGame()
    {
        stateMachine.ChangeState(GameStates.Initializing); // Chuyển sang trạng thái khởi tạo
        StartCoroutine(InitializeGameCoroutine()); // Bắt đầu quá trình setup game
        buttonpanel.SetActive(false);
    }

    /// <summary>
    /// Xử lý khi Player click vào một lá bài
    /// </summary>
    public void OnCardClicked(Card card)
    {
        if (!CanPlayerInteract()) return; // Kiểm tra có thể tương tác không

        // Case 1: Click vào bài trên tay để đánh ra
        if (playerHand.Contains(card) && currentPlayedCard == null)
        {
            HandlePlayCard(card);
        }
        // Case 2: Click vào bài trên bàn để lật (sau khi đã đánh bài)
        else if (deck.Contains(card) && currentPlayedCard != null)
        {
            HandleFlipCard(card);
        }
    }

    /// <summary>
    /// Kết thúc game - có thể gọi từ bên ngoài
    /// </summary>
    public void EndGame()
    {
        stateMachine.ChangeState(GameStates.Finished);
    }
    #endregion

    #region Game Flow - Luồng game chính
    /// <summary>
    /// Coroutine khởi tạo game: Reset -> Chia bài -> Bắt đầu chơi
    /// </summary>
    private IEnumerator InitializeGameCoroutine()
    {
        yield return StartCoroutine(ResetGame()); // Dọn dẹp game cũ
        yield return StartCoroutine(DealCardsSequence()); // Chia bài với animation

        stateMachine.ChangeState(GameStates.Playing); // Chuyển sang trạng thái chơi
        turnManager.StartPlayerTurn(); // Player đi trước
    }

    /// <summary>
    /// Reset toàn bộ trạng thái game và tạo bài mới
    /// </summary>
    private IEnumerator ResetGame()
    {
        // Xóa các bài cũ
        ClearAllCards();
        ResetGameState();

        // Tạo phân phối bài CÂN BẰNG (không duplicate giữa deck và hand)
        var cardDistribution = CreateBalancedCardDistribution();
        yield return StartCoroutine(deck.CreateDeck(cardDistribution.deckCards, gridTransform, deckPileTransform));

        // Chia bài cho 2 tay từ các lá còn lại (đảm bảo unique!)
        playerHand.SetCards(cardDistribution.playerCards);
        npcHand.SetCards(cardDistribution.npcCards);

        yield return null;
    }

    /// <summary>
    /// Thuật toán phân phối bài CHUẨN: Đảm bảo không có bài trùng giữa deck và hands
    /// Đây là fix lớn nhất so với code gốc!
    /// </summary>
    private CardDistribution CreateBalancedCardDistribution()
    {
        // Lấy tất cả sprites và xáo trộn
        var allSprites = cardSprites.ToList();
        ShuffleList(allSprites);

        var distribution = new CardDistribution();

        // Bước 1: Deck lấy 12 lá đầu
        distribution.deckCards = allSprites.Take(gameConfig.deckSize).ToList();

        // Bước 2: Hands lấy các lá còn lại (GUARANTEED UNIQUE!)
        var remainingCards = allSprites.Skip(gameConfig.deckSize).ToList();
        ShuffleList(remainingCards); // Xáo thêm lần nữa

        distribution.playerCards = remainingCards.Take(gameConfig.maxHandSize).ToList();
        distribution.npcCards = remainingCards.Skip(gameConfig.maxHandSize).Take(gameConfig.maxHandSize).ToList();

        return distribution;
    }

    /// <summary>
    /// Chia bài với animation đẹp mắt
    /// </summary>
    private IEnumerator DealCardsSequence()
    {
        // Chia bài cho Player trước (với animation từ deck pile)
        yield return StartCoroutine(DealHandAnimation(playerHand, playerHandTransform));

        // Chia bài cho NPC (ẩn)
        yield return StartCoroutine(DealHandAnimation(npcHand, npcHandTransform));

        // Bắt đầu timer sau khi chia xong
        gameTimer.StartTimer();
    }
    #endregion

    #region Card Interactions - Tương tác với bài
    /// <summary>
    /// Xử lý khi Player đánh một lá bài từ tay
    /// </summary>
    private void HandlePlayCard(Card card)
    {
        if (currentPlayedCard != null) return; // Đã có bài được đánh rồi

        currentPlayedCard = card; // Lưu lá bài đã đánh
        playerHand.RemoveCard(card); // Xóa khỏi tay

        // Animation di chuyển đến play slot
        AnimateCardToPlaySlot(card);
        AudioManager.Instance?.PlayPlayCard(); // Sound effect

        // Reset số lần lật cho lượt mới
        turnManager.ResetFlipAttempts();
    }

    /// <summary>
    /// Xử lý khi Player lật một lá bài trên bàn
    /// </summary>
    private void HandleFlipCard(Card card)
    {
        if (turnManager.CurrentFlipAttempts >= gameConfig.maxFlipAttempts) return; // Đã lật đủ 5 lần
        if (deck.IsCardFlipped(card)) return; // Bài đã được lật rồi

        deck.FlipCard(card); // Lật bài (hiện mặt thật)
        turnManager.IncrementFlipAttempts(); // Tăng số lần lật
        AudioManager.Instance?.PlayFlipCard(); // Sound effect

        // Kiểm tra match
        if (card.IconSprite == currentPlayedCard.IconSprite)
        {
            StartCoroutine(HandleSuccessfulMatch(card)); // MATCH! -> Success flow
        }
        else if (turnManager.CurrentFlipAttempts >= gameConfig.maxFlipAttempts)
        {
            StartCoroutine(HandleFailedMatch()); // Đã lật đủ 5 lần mà không match -> Fail flow
        }
        // Nếu chưa đủ 5 lần và chưa match -> tiếp tục lật
    }

    /// <summary>
    /// Xử lý khi tìm được cặp bài giống nhau - THÀNH CÔNG
    /// </summary>
    private IEnumerator HandleSuccessfulMatch(Card matchedCard)
    {
        yield return new WaitForSeconds(0.2f);

        // Feedback thành công
        AudioManager.Instance?.PlayCorrect();
        yield return StartCoroutine(PlaySuccessAnimations(currentPlayedCard, matchedCard));

        // Loại bỏ cả 2 lá bài matched
        DestroyCard(currentPlayedCard);
        DestroyCard(matchedCard);

        // Phạt đối thủ: cộng thêm 1 lá bài (punishment system)
        var opponentHand = turnManager.CurrentTurn == Turn.Player ? npcHand : playerHand;
        AddPenaltyCard(opponentHand);

        // Reset để tiếp tục lượt (người match được chơi tiếp)
        ResetTurnState();
        deck.ResetFlippedCards(); // Úp lại tất cả bài đã lật

        // Kiểm tra điều kiện thắng/thua
        CheckGameEndConditions();
    }

    /// <summary>
    /// Xử lý khi không tìm được cặp sau 5 lần lật - THẤT BẠI
    /// </summary>
    private IEnumerator HandleFailedMatch()
    {
        yield return new WaitForSeconds(0.5f);

        // Feedback thất bại
        AudioManager.Instance?.PlayWrong();
        yield return StartCoroutine(PlayFailureAnimations());

        // Trả lá bài đã đánh về tay
        var currentHand = turnManager.CurrentTurn == Turn.Player ? playerHand : npcHand;
        var handTransform = turnManager.CurrentTurn == Turn.Player ? playerHandTransform : npcHandTransform;

        ReturnCardToHand(currentPlayedCard, currentHand, handTransform);

        // Reset và chuyển lượt
        ResetTurnState();
        deck.ResetFlippedCards(); // Úp lại tất cả bài đã lật
        turnManager.SwitchTurn(); // Đổi lượt

        // Nếu đến lượt NPC thì bắt đầu AI
        if (turnManager.CurrentTurn == Turn.NPC)
        {
            StartCoroutine(ExecuteNPCTurn());
        }
    }
    #endregion

    #region NPC AI - Trí tuệ nhân tạo NPC
    /// <summary>
    /// Thực hiện lượt của NPC - AI đơn giản nhưng fair
    /// </summary>
    private IEnumerator ExecuteNPCTurn()
    {
        // đợi đến khi cho phép chơi
        while (!canPlay)
            yield return null;

        yield return new WaitForSeconds(1f); // NPC "suy nghĩ"

        if (npcHand.CardCount == 0)
        {
            EndGameWithResult(false); // NPC hết bài -> Player thua
            yield break;
        }

        // NPC chọn một lá bài ngẫu nhiên từ tay
        var npcCard = npcHand.GetRandomCard();
        npcHand.RemoveCard(npcCard);

        // Setup lá bài NPC đánh (không reveal sprite cho Player thấy)
        currentPlayedCard = CreateNPCPlayedCard(npcCard);

        // NPC bắt đầu lật bài tìm match
        yield return StartCoroutine(ExecuteNPCFlipAttempts());
    }

    /// <summary>
    /// NPC lật bài ngẫu nhiên để tìm match - AI fair (không cheat)
    /// </summary>
    private IEnumerator ExecuteNPCFlipAttempts()
    {
        turnManager.ResetFlipAttempts();

        // NPC lật tối đa 5 lần giống như Player
        while (turnManager.CurrentFlipAttempts < gameConfig.maxFlipAttempts)
        {
            var targetCard = deck.GetRandomUnflippedCard(); // Chọn ngẫu nhiên
            if (targetCard == null) break; // Không còn bài để lật

            deck.FlipCard(targetCard); // Lật bài
            turnManager.IncrementFlipAttempts();
            AudioManager.Instance?.PlayFlipCard();

            yield return new WaitForSeconds(0.6f); // Delay để Player thấy

            // Kiểm tra match
            if (targetCard.IconSprite == currentPlayedCard.IconSprite)
            {
                yield return StartCoroutine(HandleSuccessfulMatch(targetCard)); // NPC thành công
                yield break; // NPC tiếp tục chơi
            }
        }

        // NPC không tìm được match sau 5 lần
        yield return StartCoroutine(HandleFailedMatch()); // Chuyển lượt về Player
    }
    #endregion

    #region Game End Logic - Logic kết thúc game
    /// <summary>
    /// Kiểm tra điều kiện thắng/thua sau mỗi lượt
    /// </summary>
    private void CheckGameEndConditions()
    {
        if (playerHand.CardCount == 0 && npcHand.CardCount > 0)
        {
            EndGameWithResult(true); // Player hết bài trước -> Player thắng
        }
        else if (npcHand.CardCount == 0 && playerHand.CardCount > 0)
        {
            EndGameWithResult(false); // NPC hết bài trước -> Player thua
        }
        // Nếu cả hai đều còn bài -> tiếp tục chơi
    }

    /// <summary>
    /// Kết thúc game với kết quả cụ thể - UNIFIED handling
    /// Fix: Code gốc có inconsistency giữa các cách kết thúc game
    /// </summary>
    private void EndGameWithResult(bool playerWon)
    {
        stateMachine.ChangeState(GameStates.Finished); // Chuyển state
        gameTimer.StopTimer(); // Dừng timer

        // Gửi event thống nhất cho tất cả listeners
        OnGameEnded?.Invoke(playerWon);

        // Khôi phục khả năng di chuyển của Player
        RestorePlayerMovement();

        // Phát âm thanh thích hợp
        if (playerWon)
            AudioManager.Instance?.PlayWinGame();
        else
            AudioManager.Instance?.PlayLoseGame();
    }
    #endregion

    #region Event Handlers - Xử lý events
    /// <summary>
    /// Xử lý khi có người hết thời gian
    /// Fix: Code gốc có inconsistent time-up handling
    /// </summary>
    private void HandleTimeUp(Turn turn)
    {
        // Logic THỐNG NHẤT: Ai hết thời gian thì thua
        EndGameWithResult(turn == Turn.NPC); // Nếu NPC hết giờ -> Player thắng
    }

    /// <summary>
    /// Xử lý khi đổi lượt chơi
    /// </summary>

    private void HandleTurnChanged(Turn newTurn)
    {
        canPlay = false;                   // khóa chơi
        OnTurnChanged?.Invoke(newTurn);    // UI show turnText
        gameTimer.SwitchTimer(newTurn);    // đổi timer
    }
    /// <summary>
    /// Xử lý khi state game thay đổi
    /// </summary>
    private void HandleStateChanged(GameStates newState)
    {
        switch (newState)
        {
            case GameStates.Initializing:
                SetPlayerMovement(false); // Khóa di chuyển khi chơi mini-game
                SetCameraTarget(cameraFocusPoint); // Zoom camera vào bàn bài
                break;

            case GameStates.Finished:
                SetPlayerMovement(true); // Mở khóa di chuyển
                SetCameraTarget(FindPlayerTransform()); // Camera follow Player lại
                break;
        }
    }
    #endregion

    #region Utility Methods - Các phương thức tiện ích
    /// <summary>
    /// Kiểm tra Player có thể tương tác không
    /// </summary>
    private bool CanPlayerInteract()
    {
        return stateMachine.CurrentState == GameStates.Playing
               && turnManager.CurrentTurn == Turn.Player;
    }

    /// <summary>
    /// Reset trạng thái lượt chơi
    /// </summary>
    private void ResetTurnState()
    {
        currentPlayedCard = null; // Xóa bài đã đánh
        turnManager.ResetFlipAttempts(); // Reset số lần lật về 0
    }

    /// <summary>
    /// Reset toàn bộ trạng thái game
    /// </summary>
    private void ResetGameState()
    {
        currentPlayedCard = null;
        turnManager.Reset(); // Reset về Player turn, flip attempts = 0
        gameTimer.Reset(); // Reset timer về 60s cho cả 2 bên
    }

    /// <summary>
    /// Fisher-Yates shuffle algorithm - thuật toán xáo trộn chuẩn
    /// </summary>
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]); // Tuple swap - C# 7.0
        }
    }

    /// <summary>
    /// Tìm Player transform trong scene
    /// </summary>
    private Transform FindPlayerTransform()
    {
        return GameObject.FindWithTag("Player")?.transform;
    }

    /// <summary>
    /// Bật/tắt khả năng di chuyển của Player
    /// </summary>
    private void SetPlayerMovement(bool canMove)
    {
        var player = GameObject.FindWithTag("Player");
        var movement = player?.GetComponent<PlayerMovement>();
        movement?.SetCanMove(canMove);
    }

    /// <summary>
    /// Thiết lập target cho camera
    /// </summary>
    private void SetCameraTarget(Transform target)
    {
        if (CameraFollow.Instance != null && target != null)
            CameraFollow.Instance.Target = target;
    }

    /// <summary>
    /// Cleanup khi object bị destroy - QUAN TRỌNG để tránh memory leaks
    /// </summary>
    void OnDestroy()
    {
        // Hủy đăng ký tất cả events để tránh null reference
        if (gameTimer != null) gameTimer.OnTimeUp -= HandleTimeUp;
        if (turnManager != null) turnManager.OnTurnChanged -= HandleTurnChanged;
        if (stateMachine != null) stateMachine.OnStateChanged -= HandleStateChanged;
    }
    #endregion

    #region Animation Methods - Phương thức animation (rút gọn để tập trung vào logic)
    /// <summary>
    /// Animation di chuyển bài từ tay đến play slot
    /// </summary>
    private void AnimateCardToPlaySlot(Card card)
    {
        card.transform.SetParent(playedCardSlot);
        card.transform.DOLocalMove(Vector3.zero, 0.3f).SetEase(Ease.OutQuad);
        card.transform.DOLocalRotate(Vector3.zero, 0.3f).SetEase(Ease.OutQuad);
        card.transform.DOScale(Vector3.one * 1.2f, 0.3f).SetEase(Ease.OutQuad);
    }

    /// <summary>
    /// Animation khi match thành công
    /// </summary>
    private IEnumerator PlaySuccessAnimations(Card playedCard, Card matchedCard)
    {
        // TODO: Implement success animation (scale, shake, fade out)
        yield return new WaitForSeconds(0.5f);
    }

    /// <summary>
    /// Animation khi match thất bại
    /// </summary>
    private IEnumerator PlayFailureAnimations()
    {
        // TODO: Implement failure animation (shake, red tint, etc.)
        yield return new WaitForSeconds(0.4f);
    }

    /// <summary>
    /// Trả bài về tay với animation
    /// </summary>
    private void ReturnCardToHand(Card card, PlayerHand hand, Transform handTransform)
    {
        hand.AddCard(card);
        card.transform.SetParent(handTransform);
        // TODO: Animation logic...
    }

    /// <summary>
    /// Animation chia bài
    /// </summary>
    private IEnumerator DealHandAnimation(PlayerHand hand, Transform handTransform)
    {
        // TODO: Deal animation implementation
        yield return new WaitForSeconds(0.1f);
    }

    /// <summary>
    /// Tạo visual representation của bài NPC đã đánh (không reveal actual sprite)
    /// </summary>
    private Card CreateNPCPlayedCard(Card originalCard)
    {
        // TODO: Create displayed version for play slot
        return originalCard;
    }

    /// <summary>
    /// Thêm bài phạt cho đối thủ
    /// </summary>
    private void AddPenaltyCard(PlayerHand hand)
    {
        // TODO: Add penalty card logic
    }

    /// <summary>
    /// Hủy một lá bài an toàn
    /// </summary>
    private void DestroyCard(Card card)
    {
        if (card != null) Destroy(card.gameObject);
    }

    /// <summary>
    /// Xóa tất cả bài hiện tại
    /// </summary>
    private void ClearAllCards()
    {
        // Dọn dẹp tất cả bài cũ
        foreach (Transform child in gridTransform) Destroy(child.gameObject);
        foreach (Transform child in playerHandTransform) Destroy(child.gameObject);
        foreach (Transform child in npcHandTransform) Destroy(child.gameObject);
        if (currentPlayedCard != null) Destroy(currentPlayedCard.gameObject);
    }

    /// <summary>
    /// Khôi phục khả năng di chuyển của Player sau khi kết thúc mini-game
    /// </summary>
    private void RestorePlayerMovement()
    {
        SetPlayerMovement(true);
    }
    #endregion
}

#region Supporting Classes and Enums - Các class và enum hỗ trợ
/// <summary>
/// Cấu hình game - có thể serialize trong Inspector để dễ balance
/// </summary>
[System.Serializable]
public class GameConfig
{
    [Tooltip("Số lá bài trên bàn")]
    public int deckSize = 12;

    [Tooltip("Số lá bài tối đa trên tay mỗi người")]
    public int maxHandSize = 3;

    [Tooltip("Số lần lật tối đa mỗi lượt")]
    public int maxFlipAttempts = 5;

    [Tooltip("Thời gian mỗi người (giây)")]
    public float turnTimeLimit = 60f;
}

/// <summary>
/// Struct chứa phân phối bài cho deck và 2 hands
/// Đảm bảo không có duplicate giữa các vùng
/// </summary>
public struct CardDistribution
{
    public List<Sprite> deckCards;   // 12 lá cho bàn
    public List<Sprite> playerCards; // 3 lá cho Player
    public List<Sprite> npcCards;    // 3 lá cho NPC
}

/// <summary>
/// Enum trạng thái chính của game
/// </summary>
public enum GameStates
{
    Menu,         // Ở menu chính
    Initializing, // Đang setup game (chia bài, animation)
    Playing,      // Đang chơi
    Finished      // Đã kết thúc
}

/// <summary>
/// Enum lượt chơi
/// </summary>
public enum Turn
{
    Player, // Lượt của người chơi
    NPC     // Lượt của máy
}

/// <summary>
/// State Machine quản lý trạng thái game một cách clean
/// Sử dụng Observer pattern với events
/// </summary>
public class CardGameStateMachine
{
    public GameStates CurrentState { get; private set; } // Read-only từ bên ngoài
    public event Action<GameStates> OnStateChanged; // Event khi state thay đổi

    /// <summary>
    /// Chuyển state và notify các listeners
    /// </summary>
    public void ChangeState(GameStates newState)
    {
        if (CurrentState != newState) // Chỉ fire event khi state thật sự thay đổi
        {
            CurrentState = newState;
            OnStateChanged?.Invoke(newState);
        }
    }
}

/// <summary>
/// Hệ thống timer chuyên dụng cho game
/// Quản lý thời gian của cả 2 người chơi
/// </summary>
public class GameTimer
{
    public float PlayerTimeRemaining { get; private set; } // Thời gian còn lại của Player
    public float NPCTimeRemaining { get; private set; }    // Thời gian còn lại của NPC
    public event Action<Turn> OnTimeUp; // Event khi ai đó hết thời gian

    private float timeLimit;  // Thời gian giới hạn (ví dụ 60s mỗi bên)
    private bool isRunning;   // Cờ báo timer có đang chạy hay không
    private Turn activeTurn;  // Lượt hiện tại đang đếm (Player hay NPC)

    // Constructor - khởi tạo Timer với giới hạn thời gian
    // timeLimit: số giây tối đa cho mỗi người chơi
    public GameTimer(float timeLimit)
    {
        this.timeLimit = timeLimit; // Lưu lại thời gian giới hạn
        Reset(); // Reset timer về giá trị ban đầu (PlayerTimeRemaining = NPCTimeRemaining = timeLimit)
    }

    // Bắt đầu chạy timer (cho phép UpdateTimer bắt đầu trừ dần thời gian)
    public void StartTimer()
    {
        isRunning = true;
    }

    // Tạm dừng timer (dừng đếm ngược)
    public void StopTimer()
    {
        isRunning = false;
    }

    // Đổi lượt timer (khi đến lượt người khác)
    public void SwitchTimer(Turn newTurn)
    {
        activeTurn = newTurn; // Cập nhật lượt hiện tại
    }

    // Cập nhật timer theo từng frame (deltaTime là số giây trôi qua kể từ frame trước)
    // currentTurn để xác định đang trừ thời gian của Player hay NPC
    public void UpdateTimer(float deltaTime, Turn currentTurn)
    {
        if (!isRunning) return; // Nếu timer không chạy thì bỏ qua

        if (currentTurn == Turn.Player)
        {
            PlayerTimeRemaining -= deltaTime; // Giảm thời gian của Player
            if (PlayerTimeRemaining <= 0) // Nếu hết thời gian
            {
                PlayerTimeRemaining = 0;
                isRunning = false; // Dừng timer
                OnTimeUp?.Invoke(Turn.Player); // Gửi event báo Player hết giờ
            }
        }
        else
        {
            NPCTimeRemaining -= deltaTime; // Giảm thời gian của NPC
            if (NPCTimeRemaining <= 0) // Nếu hết thời gian
            {
                NPCTimeRemaining = 0;
                isRunning = false; // Dừng timer
                OnTimeUp?.Invoke(Turn.NPC); // Gửi event báo NPC hết giờ
            }
        }
    }

    // Reset toàn bộ timer về trạng thái ban đầu
    public void Reset()
    {
        PlayerTimeRemaining = timeLimit; // Player có lại full thời gian
        NPCTimeRemaining = timeLimit;    // NPC cũng vậy
        isRunning = false;               // Ban đầu chưa chạy
        activeTurn = Turn.Player;        // Mặc định bắt đầu từ Player
    }
    #endregion

}*/

using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Playables;
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
    [SerializeField] GameObject dialogue;
    [SerializeField] TMPro.TextMeshProUGUI playerTimerText;
    [SerializeField] TMPro.TextMeshProUGUI npcTimerText;

    [SerializeField] private GameObject turnPanel;
    [SerializeField] private TMPro.TextMeshProUGUI turnText;

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
        GameStateManager.Instance.SetState(GameState.MiniGame);
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

        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            var move = player.GetComponent<PlayerMovement>();
            if (move != null)
                move.SetCanMove(false); // ← KHÓA DI CHUYỂN
        }
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
        UpdateTurnUI();

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
                isTimerRunning = false;
                GameOverPanel.Instance.ShowGameOver();  // Player thua
            }
        }
        else if (currentTurn == Turn.NPC)
        {
            npcTimeRemaining -= delta;
            if (npcTimeRemaining <= 0)
            {
                isTimerRunning = false;
                EndGame(true); // player thang
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
        //Debug.Log($"NPC đánh lá: {npcCard.iconSprite.name}, còn {npcHand.Count} lá");

        playedCard = npcCard;
        return npcCard;
    }

    IEnumerator FillDeckCards(List<Sprite> deckSprites)
    {
        for (int i = 0; i < 12 && i < deckSprites.Count; i++)
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
        for (int i = 0; i < 3; i++)
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

        //Debug.Log($"NPC có {npcHand.Count} lá bài.");
        yield return null;
    }

    IEnumerator FillPlayerHand(List<Sprite> deckSprites)
    {
        for (int i = 0; i < 3; i++)
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
                UpdateTurnUI();
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
                UpdateTurnUI();
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
        //Debug.Log($"NPC bị cộng thêm 1 lá. Hiện có {npcHand.Count} lá.");
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
            EndGame(false);
        }
    }

    void UpdateTimerUI()
    {
        playerTimerText.text = $"EREN: {Mathf.CeilToInt(playerTimeRemaining)}s";
        npcTimerText.text = $"JACK: {Mathf.CeilToInt(npcTimeRemaining)}s";
    }

    void UpdateTurnUI()
    {
        if (turnPanel == null || turnText == null) return;

        string message = currentTurn == Turn.Player ? "EREN" : "JACK";
        turnText.text = message;

        isPlayerInputEnabled = false;

        // Đổi màu text timer
        if (playerTimerText != null && npcTimerText != null)
        {
            if (currentTurn == Turn.Player)
            {
                playerTimerText.color = HexToColor("00BBD4");
                npcTimerText.color = HexToColor("2B474B");
            }
            else
            {
                npcTimerText.color = HexToColor("00BBD4");
                playerTimerText.color = HexToColor("2B474B");
            }
        }

        // Hiện panel trong 1–2 giây (tuỳ thích)
        turnPanel.SetActive(true);
        CanvasGroup cg = turnPanel.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 1;
            cg.DOFade(0, 1.5f).SetDelay(1.0f).OnComplete(() =>
            {
                turnPanel.SetActive(false);
                // Khi panel tắt → cho phép input nếu là player
                isPlayerInputEnabled = (currentTurn == Turn.Player);
            });
        }
        else
        {
            // Không có CanvasGroup thì tắt thủ công
            Invoke(nameof(HideTurnPanel), 2f);
        }
    }
    Color HexToColor(string hex)
    {
        Color color;
        if (ColorUtility.TryParseHtmlString("#" + hex, out color))
            return color;
        return Color.white;
    }

    void HideTurnPanel()
    {
        turnPanel.SetActive(false);
        isPlayerInputEnabled = (currentTurn == Turn.Player);
    }

    //void HideTurnPanel()
    //{
    //    turnPanel.SetActive(false);
    //}

    void EndGame(bool playerWin)
    {
        GameStateManager.Instance.ResetToGameplay();
        Debug.Log("Da end game bai");
        isTimerRunning = false;
        gamePanel.SetActive(false);
        finishPanel.SetActive(false);
        buttonPanel.SetActive(!playerWin);
        banPanel.SetActive(!playerWin);
        CameraFollow.Instance.Target = playerTransform;

        if (playerWin)
        {
            AudioManager.Instance?.PlayWinGame();
            //if (barrierObject != null) barrierObject.SetActive(false);
            // if (barrierObject != null)
            // {
            //     // Bật trigger nếu có Collider
            //     var collider = barrierObject.GetComponent<Collider2D>();
            //     if (collider != null) collider.isTrigger = true;

            //     // Gọi animation mở cửa nếu có Animator
            //     var animator = barrierObject.GetComponent<Animator>();
            //     if (animator != null) animator.SetTrigger("open");

            //     dialogue.SetActive(true);
            // }
            dialogue.SetActive(true);
        }
        else
        {
            AudioManager.Instance?.PlayLoseGame();
        }

        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            var move = player.GetComponent<PlayerMovement>();
            if (move != null)
                move.SetCanMove(true); // ← MỞ LẠI DI CHUYỂN
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


