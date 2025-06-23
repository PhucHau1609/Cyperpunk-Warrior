using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "mapdemo2" && !hasStarted)
        {
            hasStarted = true;

            // Tự động tìm Player nếu chưa gán
            if (player == null)
                player = FindAnyObjectByType<PlayerMovement>();

            // Tìm NPC Pet nếu chưa gán
            if (pet == null)
                pet = FindAnyObjectByType<FloatingFollower>();

            if (pet != null)
            {
                petControl = pet.GetComponent<PetManualControl>();
                petAnimator = pet.GetComponent<Animator>();

                petControl.enabled = false;

                StartInitialDialogue();
            }
            else
            {
                Debug.LogWarning("Không tìm thấy Pet trong map3!");
            }
        }
    }

    void StartInitialDialogue()
    {
        if (initialDialogue != null)
        {
            DialogueManager.Instance.onDialogueEnd = SwitchToPetControl;
            DialogueManager.Instance.StartDialogue(initialDialogue.lines);
        }
    }

    void SwitchToPetControl()
    {
        player.SetCanMove(false);
        petControl.enabled = true;
        // Tắt script theo dõi Player
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
            petAnimator.SetTrigger("Idle"); // Cần có animation "Die" trong Animator
        }

        Invoke(nameof(ReturnControlToPlayer), 2f); // Delay nếu muốn chờ animation
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