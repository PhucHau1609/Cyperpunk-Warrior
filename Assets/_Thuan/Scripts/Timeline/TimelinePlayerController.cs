using UnityEngine;
using UnityEngine.Playables;

public class TimelinePlayerController : MonoBehaviour
{
    [Header("Player References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerMovement2 playerMovement2;
    [SerializeField] private CharacterController2D characterController;
    
    [Header("Timeline Settings")]
    [SerializeField] public PlayableDirector playableDirector;
    [SerializeField] private bool lockPlayerOnStart = true;
    [SerializeField] private bool unlockPlayerOnFinish = true;
    
    private bool wasPlayerAbleToMove = true;

    private void Start()
    {
        // Tự động tìm các component nếu chưa được gán
        if (playerMovement == null)
            playerMovement = FindFirstObjectByType<PlayerMovement>();

        if (playerMovement2 == null)
            playerMovement2 = FindFirstObjectByType<PlayerMovement2>();
            
        if (characterController == null)
            characterController = FindFirstObjectByType<CharacterController2D>();

        if (playableDirector == null)
            playableDirector = GetComponent<PlayableDirector>();

        // Đăng ký sự kiện Timeline
        if (playableDirector != null)
        {
            playableDirector.played += OnTimelineStart;
            playableDirector.stopped += OnTimelineStop;
        }
    }

    private void OnDestroy()
    {
        // Hủy đăng ký sự kiện khi object bị destroy
        if (playableDirector != null)
        {
            playableDirector.played -= OnTimelineStart;
            playableDirector.stopped -= OnTimelineStop;
        }
    }

    /// <summary>
    /// Được gọi khi Timeline bắt đầu chạy
    /// </summary>
    private void OnTimelineStart(PlayableDirector director)
    {
        if (lockPlayerOnStart)
        {
            LockPlayerMovement();
        }
    }

    /// <summary>
    /// Được gọi khi Timeline kết thúc
    /// </summary>
    private void OnTimelineStop(PlayableDirector director)
    {
        if (unlockPlayerOnFinish)
        {
            UnlockPlayerMovement();
        }
    }

    /// <summary>
    /// Khóa di chuyển của Player
    /// </summary>
    public void LockPlayerMovement()
    {
        if (playerMovement != null)
        {
            wasPlayerAbleToMove = playerMovement.canMove;
            playerMovement.SetCanMove(false);
        }
        if (playerMovement2 != null)
        {
            wasPlayerAbleToMove = playerMovement2.canMove;
            playerMovement2.SetCanMove(false);
        }
    }

    /// <summary>
    /// Mở khóa di chuyển của Player
    /// </summary>
    public void UnlockPlayerMovement()
    {
        if (playerMovement != null)
        {
            playerMovement.SetCanMove(wasPlayerAbleToMove);
        }
        if (playerMovement2 != null)
        {
            playerMovement2.SetCanMove(wasPlayerAbleToMove);
        }
        GameStateManager.Instance.ResetToGameplay();
    }

    /// <summary>
    /// Force unlock - luôn mở khóa bất kể trạng thái trước đó
    /// </summary>
    public void ForceUnlockPlayerMovement()
    {
        if (playerMovement != null)
        {
            playerMovement.SetCanMove(true);
        }
        if (playerMovement2 != null)
        {
            playerMovement2.SetCanMove(true);
        }
    }

    /// <summary>
    /// Kiểm tra trạng thái di chuyển của Player
    /// </summary>
    public bool IsPlayerMovementLocked()
    {
        if (playerMovement != null)
        return !playerMovement.canMove;

        if (playerMovement2 != null)
            return !playerMovement2.canMove;

        return false;
    }

    /// <summary>
    /// Bật/tắt tự động khóa khi Timeline start
    /// </summary>
    public void SetAutoLockOnStart(bool enable)
    {
        lockPlayerOnStart = enable;
    }

    /// <summary>
    /// Bật/tắt tự động mở khóa khi Timeline finish
    /// </summary>
    public void SetAutoUnlockOnFinish(bool enable)
    {
        unlockPlayerOnFinish = enable;
    }

    /// <summary>
    /// Phát Timeline và khóa Player
    /// </summary>
    public void PlayTimelineAndLockPlayer()
    {
        GameStateManager.Instance.SetState(GameState.MiniGame);
        LockPlayerMovement();
        if (playableDirector != null)
        {
            playableDirector.Play();
        }
    }

    /// <summary>
    /// Dừng Timeline và mở khóa Player
    /// </summary>
    public void StopTimelineAndUnlockPlayer()
    {
        if (playableDirector != null)
        {
            playableDirector.Stop();
        }
        UnlockPlayerMovement();
    }
}