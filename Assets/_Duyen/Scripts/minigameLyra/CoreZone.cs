using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CoreZone : MonoBehaviour
{
    [Header("Reward")]
    [SerializeField] protected ItemCode rewardCore;
    [SerializeField] protected Vector3 spawnCore;

    [Header("Minigame")]
    public GameObject minigamePrefab; // Kéo prefab MinigameCoreUI vào đây trong Inspector
    public float targetSpeed = 100f;  // Tốc độ riêng cho mỗi lõi

    //private bool lyraInside = false;
    private bool canBeInteracted = false;
    //private SpriteRenderer spriteRenderer;
    private CoreManager coreManager;
    //private int currentStage = 0;
    private GameObject spawnedMinigame;

    private bool minigameRunning = false;
    private Image npcHealthFill;

    void Start()
    {
        //spriteRenderer = GetComponent<SpriteRenderer>();
        coreManager = FindAnyObjectByType<CoreManager>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!canBeInteracted) return;

        if (other.CompareTag("NPC"))
        {
            //lyraInside = true;

            if (minigamePrefab != null && spawnedMinigame == null)
            {
                // Lấy health fill từ LyraHealth
                GameObject lyraObj = GameObject.FindGameObjectWithTag("NPC");
                if (lyraObj != null)
                {
                    var lyraHealth = lyraObj.GetComponent<LyraHealth>();
                    if (lyraHealth != null && lyraHealth.healthBarUI != null)
                        npcHealthFill = lyraHealth.healthBarUI;
                }

                // Tạo panel minigame
                Transform canvas = GameObject.Find("bar_ca").transform;
                spawnedMinigame = Instantiate(minigamePrefab, canvas);

                var minigame = spawnedMinigame.GetComponent<CoreMinigameController>();
                minigame.targetSpeed = targetSpeed;
                minigame.lyraObject = lyraObj;

                // Khi hoàn thành minigame
                minigame.onComplete = () =>
                {
                    minigameRunning = false;
                    canBeInteracted = false;
                    coreManager.MarkCoreAsComplete(this);
                    ItemsDropManager.Instance.DropItem(rewardCore, 1, this.transform.position + spawnCore);
                    Destroy(spawnedMinigame);
                    StartCoroutine(OpenDoorSequence());
                };

                // Bắt đầu minigame
                minigameRunning = true;
                minigame.StartMinigame();

                // Chạy kiểm tra máu NPC
                StartCoroutine(CheckNpcHealthWhileMinigame());
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!canBeInteracted) return;

        if (other.CompareTag("NPC"))
        {
            //lyraInside = false;
        }
    }

    public void SetActiveLogic(bool active)
    {
        canBeInteracted = active;
    }

    private IEnumerator OpenDoorSequence()
    {
        // Delay 0.2 giây
        yield return new WaitForSeconds(0.2f);

        // Chạy animation mở cửa (ví dụ dùng Animator)
        Animator doorAnimator = GetComponent<Animator>();
        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger("Open");
        }

        yield return new WaitForSeconds(0.5f); // chỉnh theo thời gian animation
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (var col in colliders)
        {
            if (!col.isTrigger) // collider chặn đường
            {
                col.enabled = false; // tắt để mở đường
                Debug.Log("Door collision disabled — NPC can pass now!");
            }
        }
        
    }

    private IEnumerator CheckNpcHealthWhileMinigame()
    {
        while (minigameRunning)
        {
            if (npcHealthFill != null && npcHealthFill.fillAmount <= 0f)
            {
                // NPC chết → thua game
                minigameRunning = false;
                Debug.Log("NPC chết → thua game!");

                //minigameRunning = false;
                //canBeInteracted = false;
                //coreManager.MarkCoreAsComplete(this);
                Destroy(spawnedMinigame);
                //StartCoroutine(OpenDoorSequence());
                // Báo SceneController xử lý thua
                //SceneController sceneController = FindAnyObjectByType<SceneController>();
                //sceneController?.HandleGameOver();
                //canBeInteracted = true;
                yield break;
            }
            yield return null;
        }
    }
}
