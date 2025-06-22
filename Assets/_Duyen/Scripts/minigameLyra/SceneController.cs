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
        // Tắt script theo dõi Player
        FloatingFollower follow = pet.GetComponent<FloatingFollower>();
        if (follow != null)
            follow.enabled = false;
        petControl.enabled = true;
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
    }
}
//public DialogueData introDialogue;      // Thoại đầu
    //public DialogueData deathDialogue;      // Thoại sau khi chạm công tắc
    //public string sceneToWatch = "mapdemo2";

    //private GameObject player;
    //private GameObject pet;
    //private bool controlGivenToPet = false;

    //void Start()
    //{
    //    if (SceneManager.GetActiveScene().name == sceneToWatch)
    //    {
    //        StartCoroutine(HandleSceneStart());
    //    }
    //}

    //IEnumerator HandleSceneStart()
    //{
    //    yield return new WaitForSeconds(1f); // Đợi mọi thứ load xong

    //    player = GameObject.FindGameObjectWithTag("Player");
    //    pet = GameObject.FindGameObjectWithTag("NPC");

    //    if (player != null)
    //        player.GetComponent<PlayerMovement>().SetCanMove(false);

    //    DialogueManager.Instance.onDialogueEnd = GiveControlToPet;
    //    DialogueManager.Instance.StartDialogue(introDialogue, pet.transform);
    //}

    //void GiveControlToPet()
    //{
    //    if (pet == null || player == null) return;

    //    player.GetComponent<PlayerMovement>().SetCanMove(false);

    //    pet.GetComponent<FloatingFollower>().enabled = false;
    //    pet.GetComponent<PetManualControl>().enabled = true;

    //    controlGivenToPet = true;
    //}

    //// Gọi từ công tắc khi pet chạm vào
    //public void OnPetTouchedSwitch()
    //{
    //    if (!controlGivenToPet) return;

    //    pet.GetComponent<PetManualControl>().enabled = false;
    //    DialogueManager.Instance.onDialogueEnd = PetDiesAndGiveBackControl;
    //    DialogueManager.Instance.StartDialogue(deathDialogue, pet.transform);
    //}

    //void PetDiesAndGiveBackControl()
    //{
    //    if (pet != null)
    //    {
    //        Animator anim = pet.GetComponent<Animator>();
    //        if (anim != null) anim.SetTrigger("Die");
    //    }

    //    if (player != null)
    //        player.GetComponent<PlayerMovement>().SetCanMove(true);

    //    controlGivenToPet = false;
    //}