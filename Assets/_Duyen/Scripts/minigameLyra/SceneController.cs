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

    //private bool hasStarted = false;

    public UnityEvent onReturnToPlayer;


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

    public void ReturnControlToPlayer()
    {
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

        //Tắt các object bẩy
        foreach (GameObject obj in objectsToDisableOnReturn)
        {
            if (obj != null)
                obj.SetActive(false);
        }

        onReturnToPlayer?.Invoke();

        // Bật lại trigger để Pet có thể xuyên vật thể khi bay theo Player
        foreach (var col in pet.GetComponents<Collider2D>())
            col.isTrigger = true;

        foreach (GameObject obj in animatorObjectsToDisable)
        {
            if (obj != null)
            {
                Animator anim = obj.GetComponent<Animator>();
                if (anim != null)
                    anim.enabled = false;
            }
        }

    }

}
