using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Quản lý hiển thị thoại khi nhận các sự kiện trong game (dựa trên Observer Pattern).
/// Có thể mở rộng thêm các sự kiện khác dễ dàng.
/// </summary>
public class DialogueEventTrigger : MonoBehaviour
{
    [Header("Lyra Setup")]
    public LyraDialogueTrigger lyraTransform;

    [Header("Dialogue khi lần đầu nhặt được súng")]
    public DialogueData firstGunDialogue;

    [Header("Dialogue khi chết 2 lần ở map 2")]
    public DialogueData diedTwiceInMap2Dialogue;
    //public CharacterController2D playerPos;
    //private Transform checkPointPosition;

    [Header("Dialogue khi lần đầu nhặt được energy")]
    public DialogueData firstEnergyDialogue;

    [Header("Dialogue khi chưa nhặt được CraftingRecipe")]
    public DialogueData hasNotHaveCraftingRecipe;

    [Header("Dialogue khi lần 2 nhặt được energy")]
    public DialogueData secondEnergyDialogue;

    //private int deathCountAtMap2 = 0;

    private void OnEnable()
    {
        ObserverManager.Instance.AddListener(EventID.FirstGunPickedUp, OnFirstGunPickedUp);
        ObserverManager.Instance.AddListener(EventID.FirstEnergyPickedUp, OnFirstEnergyPickedUp);
        ObserverManager.Instance.AddListener(EventID.SecondEnergyPickedUp, OnSecondEnergyPickedUp);

        ObserverManager.Instance.AddListener(EventID.PlayerDied, OnPlayerDied);
        ObserverManager.Instance.AddListener(EventID.NotHasCraftingRecipeInInventory, NotHasCraftingRecipeInInventory);

    }

    private void OnDisable()
    {
        ObserverManager.Instance.RemoveListener(EventID.FirstGunPickedUp, OnFirstGunPickedUp);
        ObserverManager.Instance.RemoveListener(EventID.FirstEnergyPickedUp, OnFirstEnergyPickedUp);
        ObserverManager.Instance.RemoveListener(EventID.SecondEnergyPickedUp, OnSecondEnergyPickedUp);

        ObserverManager.Instance.RemoveListener(EventID.PlayerDied, OnPlayerDied);
        ObserverManager.Instance.RemoveListener(EventID.NotHasCraftingRecipeInInventory, NotHasCraftingRecipeInInventory);
    }

    private void OnFirstGunPickedUp(object param)
    {
        if (firstGunDialogue == null) return;

        DialogueManager.Instance.StartDialogue(firstGunDialogue, lyraTransform.NPCTransform);
        QuestEventBus.Raise("near_door");
    }

    private void OnFirstEnergyPickedUp(object param)
    {
        if (firstEnergyDialogue == null) return;

        DialogueManager.Instance.StartDialogue(firstEnergyDialogue, lyraTransform.NPCTransform);
    }

    private void OnSecondEnergyPickedUp(object param)
    {
        if (secondEnergyDialogue == null) return;

        DialogueManager.Instance.StartDialogue(secondEnergyDialogue, lyraTransform.NPCTransform);
    }

    private void NotHasCraftingRecipeInInventory(object param)
    {
        if (hasNotHaveCraftingRecipe == null) return;
        DialogueManager.Instance.StartDialogue(hasNotHaveCraftingRecipe, lyraTransform.NPCTransform);

    }

    private void OnPlayerDied(object param)
    {
       /* string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == SpawnSceneName.map2level4.ToString())
        {
            deathCountAtMap2++;

            if (deathCountAtMap2 == 2 && diedTwiceInMap2Dialogue != null)
            {

                Debug.Log("Hien thi dialogue ne");
                DialogueManager.Instance.StartDialogue(diedTwiceInMap2Dialogue, lyraTransform.NPCTransform);
                Debug.Log("Hien thi dialogue xong");

            }
        }*/
    }
}
