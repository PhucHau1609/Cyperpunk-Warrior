using UnityEngine;
using UnityEngine.Playables;

public class TimelinePlayerController : MonoBehaviour
{
    [Header("Player References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Player player;
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

        if (player == null)
            player = FindFirstObjectByType<Player>();
            
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
        if (player != null)
        {
            wasPlayerAbleToMove = player.canMove;
            player.SetCanMove(false);
        }
        
        Debug.Log("Player movement locked during cutscene");
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
        if (player != null)
        {
            player.SetCanMove(wasPlayerAbleToMove);
        }
        
        Debug.Log("Player movement unlocked after cutscene");
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
        if (player != null)
        {
            player.SetCanMove(true);
        }
        
        Debug.Log("Player movement force unlocked");
    }

    /// <summary>
    /// Kiểm tra trạng thái di chuyển của Player
    /// </summary>
    public bool IsPlayerMovementLocked()
    {
        if (playerMovement != null)
        return !playerMovement.canMove;

        if (player != null)
            return !player.canMove;

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