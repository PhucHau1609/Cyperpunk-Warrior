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

    // Method để check xem có đang trong mini game không
    public bool IsInMiniGame()
    {
        return wasInMiniGame && petControl != null && petControl.enabled;
    }

    // Overload method để backwards compatibility
    public void ReturnControlToPlayer()
    {
        ReturnControlToPlayer(true); // Default: mini game completed
    }

    public void ReturnControlToPlayer(bool miniGameCompleted)
    {
        Debug.Log($"[SceneController] ReturnControlToPlayer called - miniGameCompleted: {miniGameCompleted}");
        
        wasInMiniGame = false;
        
        GameStateManager.Instance.ResetToGameplay();
        InventoryUIHandler.Instance.ToggleIconWhenPlayMiniGame();
        player.SetCanMove(true);

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

        // Bật lại chế độ follow Player
        FloatingFollower follow = pet.GetComponent<FloatingFollower>();
        if (follow != null)
            follow.enabled = true;

        // Camera quay lại Player
        if (CameraFollow.Instance != null)
            CameraFollow.Instance.Target = player.transform;

        // QUAN TRỌNG: Chỉ tắt các object khi THỰC SỰ hoàn thành mini game
        if (miniGameCompleted)
        {
            Debug.Log("[SceneController] Mini game completed - disabling obstacles");
            
            //Tắt các object bẩy
            foreach (GameObject obj in objectsToDisableOnReturn)
            {
                if (obj != null)
                    obj.SetActive(false);
            }

            foreach (GameObject obj in animatorObjectsToDisable)
            {
                if (obj != null)
                {
                    Animator anim = obj.GetComponent<Animator>();
                    if (anim != null)
                        anim.enabled = false;
                }
            }
            
            // Tắt GameOverManager khi hoàn thành
            GameOverManager gom = FindAnyObjectByType<GameOverManager>();
            if (gom != null)
            {
                gom.enabled = false;
            }
        }
        else
        {
            Debug.Log("[SceneController] Mini game restarted - keeping obstacles active");
        }

        onReturnToPlayer?.Invoke();

        // Bật lại trigger để Pet có thể xuyên vật thể khi bay theo Player
        foreach (var col in pet.GetComponents<Collider2D>())
            col.isTrigger = true;
    }
}