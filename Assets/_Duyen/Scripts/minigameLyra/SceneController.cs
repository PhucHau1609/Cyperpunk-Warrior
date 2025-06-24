using System.Collections;
using UnityEngine;

public class SceneController : MonoBehaviour
{
    public PlayerMovement player;
    public DialogueData initialDialogue;
    public DialogueData afterSwitchDialogue;

    private FloatingFollower pet;
    private PetManualControl petControl;
    private Animator petAnimator;

    private bool hasStarted = false;

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

            // Không gọi StartInitialDialogue() nữa ở đây
        }
        else
        {
            Debug.LogWarning("Không tìm thấy Pet trong map3!");
        }
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
        player.SetCanMove(false);
        petControl.enabled = true;

        FloatingFollower follow = pet.GetComponent<FloatingFollower>();
        if (follow != null)
            follow.enabled = false;

        PetShooting petShooting = pet.GetComponent<PetShooting>();
        if (petShooting != null)
            petShooting.enabled = true;
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

    void ReturnControlToPlayer()
    {
        player.SetCanMove(true);
        if (petControl != null)
            petControl.enabled = false;

        PetShooting petShooting = pet.GetComponent<PetShooting>();
        if (petShooting != null)
            petShooting.enabled = false;
    }
}
