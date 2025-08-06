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
    public CharacterController2D playerPos;
    private Transform checkPointPosition;

    [Header("Dialogue khi lần đầu nhặt được energy")]
    public DialogueData firstEnergyDialogue;

    private int deathCountAtMap2 = 0;

    private void OnEnable()
    {
        ObserverManager.Instance.AddListener(EventID.FirstGunPickedUp, OnFirstGunPickedUp);
        ObserverManager.Instance.AddListener(EventID.FirstEnergyPickedUp, OnFirstEnergyPickedUp);
        ObserverManager.Instance.AddListener(EventID.PlayerDied, OnPlayerDied);
    }

    private void OnDisable()
    {
        ObserverManager.Instance.RemoveListener(EventID.FirstGunPickedUp, OnFirstGunPickedUp);
        ObserverManager.Instance.RemoveListener(EventID.FirstEnergyPickedUp, OnFirstEnergyPickedUp);
        ObserverManager.Instance.RemoveListener(EventID.PlayerDied, OnPlayerDied);
    }

    private void OnFirstGunPickedUp(object param)
    {
        if (firstGunDialogue == null) return;

        DialogueManager.Instance.StartDialogue(firstGunDialogue, lyraTransform.NPCTransform);
    }

    private void OnFirstEnergyPickedUp(object param)
    {
        if (firstEnergyDialogue == null) return;

        DialogueManager.Instance.StartDialogue(firstEnergyDialogue, lyraTransform.NPCTransform);
    }

    private void OnPlayerDied(object param)
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == SpawnSceneName.map2level4.ToString())
        {
            deathCountAtMap2++;
            //Thiếu logic tìm checkPointPosition
            //var checkPoint = FindFirstObjectByType<Checkpoint>();


            if (deathCountAtMap2 == 2 && diedTwiceInMap2Dialogue != null)
            {

                Debug.Log("Hien thi dialogue ne");
                DialogueManager.Instance.StartDialogue(diedTwiceInMap2Dialogue, lyraTransform.NPCTransform);
                Debug.Log("Hien thi dialogue xong");

            }
        }
    }

    // Mở rộng thêm sự kiện khác tại đây
    // Ví dụ:
    // private void OnUnlockSkillDash(object param) { ... }
    // ObserverManager.Instance.AddListener(EventID.UnlockSkill_Dash, OnUnlockSkillDash);
}
