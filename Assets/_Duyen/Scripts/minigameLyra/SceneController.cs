using System.Collections;
using UnityEngine;
using Cinemachine;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.AI;

public class SceneController : MonoBehaviour
{
    public PlayerMovement player;
    public DialogueData initialDialogue;
    public DialogueData afterSwitchDialogue;

    private FloatingFollower pet;
    private PetManualControl petControl;
    private Animator petAnimator;
    private Animator playerAnimator;
    private NavMeshAgent petAgent;

    public List<GameObject> objectsToDisableOnReturn;
    public List<GameObject> animatorObjectsToDisable;

    public UnityEvent onReturnToPlayer;

    // Lưu trữ trạng thái ban đầu cho restart mini game
    private bool wasInMiniGame = false;

    void Start()
    {
        // Setup như cũ
        if (player == null)
            player = FindAnyObjectByType<PlayerMovement>();

        if (pet == null)
            pet = FindAnyObjectByType<FloatingFollower>();

        if (pet != null)
        {
            petControl = pet.GetComponent<PetManualControl>();
            petAnimator = pet.GetComponent<Animator>();

            // Khóa điều khiển Pet ban đầu
            petControl.enabled = false;
            petAgent = pet.GetComponent<NavMeshAgent>();

            // Không gọi StartInitialDialogue() nữa ở đây
        }
        else
        {
            //Debug.LogWarning("Không tìm thấy Pet trong map3!");
        }
        if (player != null)
            playerAnimator = player.GetComponentInChildren<Animator>();
    }

    void StartInitialDialogue()
    {
        if (initialDialogue != null)
        {
            DialogueManager.Instance.StartDialogue(initialDialogue.lines);
            // KHÔNG GỌI SwitchToPetControl Ở ĐÂY
        }
    }

    public void TriggerInitialEvent()
    {
        StartInitialDialogue();
    }

    // Gọi hàm này từ TriggerZone
    public void SwitchToPetControl()
    {
        // QUAN TRỌNG: Check protection flag trước khi switch
        if (CheckpointManager.Instance != null && CheckpointManager.Instance.IsMiniGameProtected())
        {
            Debug.LogWarning("[SceneController] Mini game is protected - preventing auto-start");
            return;
        }
        
        // QUAN TRỌNG: Check nếu mini game đã hoàn thành
        if (CheckpointManager.Instance != null && CheckpointManager.Instance.miniGameCompleted)
        {
            Debug.LogWarning("[SceneController] Mini game already completed - preventing restart");
            return;
        }
        
        Debug.Log("[SceneController] Starting mini game - switching to pet control");
        
        wasInMiniGame = true;

        GameStateManager.Instance.SetState(GameState.MiniGame);
        InventoryUIHandler.Instance.ToggleIconWhenPlayMiniGame();

        if (petAgent != null)
            petAgent.enabled = false;

        if (playerAnimator != null)
        {
            playerAnimator.SetFloat("Speed", 0f);
        }
        player.SetCanMove(false);

        petControl.enabled = true;

        FloatingFollower follow = pet.GetComponent<FloatingFollower>();
        if (follow != null)
            follow.enabled = false;

        PetShooting petShooting = pet.GetComponent<PetShooting>();
        if (petShooting != null)
            petShooting.enabled = true;

        LyraHealth lyraHealth = pet.GetComponent<LyraHealth>();
        if (lyraHealth != null)
            lyraHealth.enabled = true;

        if (CameraFollow.Instance != null)
            CameraFollow.Instance.Target = pet.transform;

        // Tắt trigger để Pet không đi xuyên tường khi được điều khiển
        foreach (var col in pet.GetComponents<Collider2D>())
            col.isTrigger = false;
    }

    // Method mới để restart mini game
    public void RestartMiniGame()
    {
        Debug.Log("[SceneController] Restarting mini game...");

        // QUAN TRỌNG: Đảm bảo tắt Game Over Panel trước
        LyraHealth lyraHealth = pet.GetComponent<LyraHealth>();
        if (lyraHealth != null && lyraHealth.gameOverPanel != null)
        {
            lyraHealth.gameOverPanel.SetActive(false);
        }

        // QUAN TRỌNG: Nếu đang trong mini game, chỉ reset state không return control
        if (wasInMiniGame)
        {
            // Chỉ reset các state cần thiết, KHÔNG gọi ReturnControlToPlayer
            wasInMiniGame = false;
            Debug.Log("[SceneController] Reset mini game state without returning control");
        }

        // Đợi 1 frame để đảm bảo state được reset
        StartCoroutine(DelayedSwitchToPetControl());
    }

    // Coroutine để delay việc switch sang pet control
    private System.Collections.IEnumerator DelayedSwitchToPetControl()
    {
        yield return null; // Đợi 1 frame

        Debug.Log("[SceneController] Switching to pet control after reset...");
        SwitchToPetControl();

        Debug.Log("[SceneController] Mini game restart completed");
    }

    // Method để reset các objects trong mini game về trạng thái ban đầu
    private void ResetMiniGameObjects()
    {
        // Bật lại các objects đã bị tắt
        foreach (GameObject obj in objectsToDisableOnReturn)
        {
            if (obj != null)
                obj.SetActive(true);
        }

        // Bật lại các animators đã bị tắt
        foreach (GameObject obj in animatorObjectsToDisable)
        {
            if (obj != null)
            {
                Animator anim = obj.GetComponent<Animator>();
                if (anim != null)
                    anim.enabled = true;
            }
        }

        // Bật lại GameOverManager nếu có
        GameOverManager gom = FindAnyObjectByType<GameOverManager>();
        if (gom != null)
        {
            gom.enabled = true;
        }
    }

    public void OnPetTouchedSwitch()
    {
        petControl.enabled = false;

        if (afterSwitchDialogue != null)
        {
            DialogueManager.Instance.onDialogueEnd = KillPetAndReturnControl;
            DialogueManager.Instance.StartDialogue(afterSwitchDialogue.lines);
        }
    }

    void KillPetAndReturnControl()
    {
        if (petAnimator != null)
        {
            petAnimator.SetTrigger("Idle");
        }

        Invoke(nameof(ReturnControlToPlayer), 2f);
    }

    // Method để check xem có đang trong mini game không - IMPROVED
    public bool IsInMiniGame()
    {
        // QUAN TRỌNG: Kiểm tra chính xác state
        bool petControlActive = (petControl != null && petControl.enabled);
        bool gameStateIsMiniGame = (GameStateManager.Instance != null && 
                                   GameStateManager.Instance.CurrentState == GameState.MiniGame);
        bool miniGameNotCompleted = (CheckpointManager.Instance == null || 
                                    !CheckpointManager.Instance.miniGameCompleted);
        
        bool isInMiniGame = wasInMiniGame && petControlActive && gameStateIsMiniGame && miniGameNotCompleted;
        
        Debug.Log($"[SceneController] IsInMiniGame check: wasInMiniGame={wasInMiniGame}, petControlActive={petControlActive}, gameStateIsMiniGame={gameStateIsMiniGame}, miniGameNotCompleted={miniGameNotCompleted}, result={isInMiniGame}");
        
        return isInMiniGame;
    }

    // Overload method để backwards compatibility - IMPROVED
    public void ReturnControlToPlayer()
    {
        ReturnControlToPlayer(true); // Default: mini game completed
    }

    public void ReturnControlToPlayer(bool miniGameCompleted)
    {
        Debug.Log($"[SceneController] ReturnControlToPlayer called - miniGameCompleted: {miniGameCompleted}");

        // QUAN TRỌNG: Reset state ngay lập tức
        wasInMiniGame = false;

        // QUAN TRỌNG: Update CheckpointManager mini game completion status
        if (miniGameCompleted && CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.miniGameCompleted = true;
            Debug.Log("[SceneController] Set CheckpointManager.miniGameCompleted = true");
        }

        GameStateManager.Instance.ResetToGameplay();
        InventoryUIHandler.Instance.ToggleIconWhenPlayMiniGame();

        if (petAgent != null)
            petAgent.enabled = true;

        // Tắt điều khiển thủ công
        if (petControl != null)
            petControl.enabled = false;

        // Tắt bắn
        PetShooting petShooting = pet.GetComponent<PetShooting>();
        if (petShooting != null)
            petShooting.enabled = false;

        // Tắt máu + ẩn UI máu
        LyraHealth lyraHealth = pet.GetComponent<LyraHealth>();
        if (lyraHealth != null)
        {
            lyraHealth.enabled = false;
            if (lyraHealth.healthBarUI != null)
                lyraHealth.healthBarUI.gameObject.SetActive(false);
        }

        StartCoroutine(delay());

        // Bật lại chế độ follow Player
        FloatingFollower follow = pet.GetComponent<FloatingFollower>();
        if (follow != null)
            follow.enabled = true;

        if (miniGameCompleted)
            StartCoroutine(CameraTransitionAndDisableObjects());
    }

    // Method mới để force reset mini game state - ENHANCED
    public void ForceResetMiniGameState()
    {
        Debug.Log("[SceneController] === FORCE RESETTING MINI GAME STATE ===");
        
        // 1. Reset internal state
        wasInMiniGame = false;
        
        // 2. Disable pet control
        if (petControl != null)
        {
            petControl.enabled = false;
            Debug.Log("[SceneController] Disabled petControl");
        }
        
        // 3. Force reset GameStateManager
        if (GameStateManager.Instance != null)
        {
            Debug.Log($"[SceneController] GameStateManager before reset: {GameStateManager.Instance.CurrentState}");
            GameStateManager.Instance.ForceResetToGameplay();
            Debug.Log($"[SceneController] GameStateManager after reset: {GameStateManager.Instance.CurrentState}");
        }
        
        // 4. Disable all pet mini game components
        if (pet != null)
        {
            PetShooting petShooting = pet.GetComponent<PetShooting>();
            if (petShooting != null)
            {
                petShooting.enabled = false;
                Debug.Log("[SceneController] Disabled pet shooting");
            }
            
            LyraHealth lyraHealth = pet.GetComponent<LyraHealth>();
            if (lyraHealth != null)
            {
                lyraHealth.enabled = false;
                if (lyraHealth.healthBarUI != null)
                    lyraHealth.healthBarUI.gameObject.SetActive(false);
                Debug.Log("[SceneController] Disabled lyra health and UI");
            }
            
            // Reset pet colliders to trigger mode
            foreach (var col in pet.GetComponents<Collider2D>())
                col.isTrigger = true;
            
            // Enable pet agent
            if (petAgent != null)
            {
                petAgent.enabled = true;
                Debug.Log("[SceneController] Enabled pet NavMeshAgent");
            }
            
            // Enable pet following
            FloatingFollower follow = pet.GetComponent<FloatingFollower>();
            if (follow != null)
            {
                follow.enabled = true;
                Debug.Log("[SceneController] Enabled pet following");
            }
        }
        
        // 5. Restore player control
        if (player != null)
        {
            player.SetCanMove(true);
            Debug.Log("[SceneController] Enabled player movement");
            
            if (playerAnimator != null)
            {
                playerAnimator.SetFloat("Speed", 0f);
                Debug.Log("[SceneController] Reset player animator");
            }
        }
        
        // 6. Reset camera target to player
        if (CameraFollow.Instance != null && player != null)
        {
            CameraFollow.Instance.Target = player.transform;
            Debug.Log("[SceneController] Reset camera target to player");
        }
        
        // 7. Reset UI
        if (InventoryUIHandler.Instance != null)
        {
            // This should reset UI to normal state
            Debug.Log("[SceneController] Reset inventory UI handler");
        }
        
        Debug.Log("[SceneController] === MINI GAME STATE FORCE RESET COMPLETED ===");
    }

    // Method để debug current state
    public void DebugCurrentState()
    {
        Debug.Log($"[SceneController] === CURRENT STATE DEBUG ===");
        Debug.Log($"wasInMiniGame: {wasInMiniGame}");
        Debug.Log($"petControl.enabled: {(petControl != null ? petControl.enabled.ToString() : "NULL")}");
        Debug.Log($"GameState: {(GameStateManager.Instance != null ? GameStateManager.Instance.CurrentState.ToString() : "NULL")}");
        Debug.Log($"CheckpointManager.miniGameCompleted: {(CheckpointManager.Instance != null ? CheckpointManager.Instance.miniGameCompleted.ToString() : "NULL")}");
        Debug.Log($"IsInMiniGame(): {IsInMiniGame()}");
        Debug.Log($"[SceneController] === END DEBUG ===");
    }

    private IEnumerator delay()
    {
        // 1. Delay 1s
        yield return new WaitForSeconds(1f);
    }

    private IEnumerator CameraTransitionAndDisableObjects()
    {
        // 1. Delay trước khi trả cam
        yield return new WaitForSeconds(1f);

        Transform cam = Camera.main.transform;
        Vector3 startPos = cam.position;
        Vector3 targetPos = new Vector3(
            player.transform.position.x,
            player.transform.position.y,
            startPos.z
        );

        float duration = 3f;
        float elapsed = 0f;

        int objIndex = 0; // index object trong list

        // 2. Di chuyển camera từ từ
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            cam.position = Vector3.Lerp(startPos, targetPos, t);

            // 3. Tắt bẫy theo thứ tự dựa trên tiến trình di chuyển camera
            float progressPerObj = 1f / objectsToDisableOnReturn.Count;

            while (objIndex < objectsToDisableOnReturn.Count && t >= (objIndex + 1) * progressPerObj)
            {
                if (objectsToDisableOnReturn[objIndex] != null)
                {
                    objectsToDisableOnReturn[objIndex].SetActive(false);
                }
                objIndex++;
            }

            yield return null;
        }

        // 4. Đảm bảo tất cả obj đã tắt
        for (int i = objIndex; i < objectsToDisableOnReturn.Count; i++)
        {
            if (objectsToDisableOnReturn[i] != null)
                objectsToDisableOnReturn[i].SetActive(false);
        }

        // 5. Camera follow lại Player
        if (CameraFollow.Instance != null)
            CameraFollow.Instance.Target = player.transform;

        // Trả điều khiển cho player
        player.SetCanMove(true);

        onReturnToPlayer?.Invoke();

        foreach (var col in pet.GetComponents<Collider2D>())
            col.isTrigger = true;
    }
}