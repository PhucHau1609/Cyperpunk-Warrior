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
    private LyraHealth lyraHealth;

    // Lưu trữ trạng thái ban đầu
    private bool initialCanBeInteracted = false;
    private bool initialColliderState = true;
    
    // QUAN TRỌNG: Thêm flag để prevent auto-trigger sau restart
    private bool isResetting = false;
    private bool hasNpcInside = false;

    void Start()
    {
        //spriteRenderer = GetComponent<SpriteRenderer>();
        coreManager = FindAnyObjectByType<CoreManager>();

        // Lưu trạng thái ban đầu
        initialCanBeInteracted = canBeInteracted;

        // Lưu trạng thái collider ban đầu
        Collider2D[] colliders = GetComponents<Collider2D>();
        if (colliders.Length > 0)
        {
            // Tìm collider không phải trigger (collider chặn đường)
            foreach (var col in colliders)
            {
                if (!col.isTrigger)
                {
                    initialColliderState = col.enabled;
                    break;
                }
            }
        }

        Debug.Log($"[CoreZone] {gameObject.name} initialized - canBeInteracted: {canBeInteracted}, initialColliderState: {initialColliderState}");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("NPC"))
        {
            hasNpcInside = true;
            Debug.Log($"[CoreZone] 🚀 {gameObject.name} - NPC entered zone. canBeInteracted={canBeInteracted}, isResetting={isResetting}, minigameRunning={minigameRunning}");
        }
        
        // QUAN TRỌNG: Chỉ trigger khi không đang reset và có thể interact
        if (!canBeInteracted || isResetting || minigameRunning)
        {
            Debug.Log($"[CoreZone] ❌ {gameObject.name} - BLOCKED: canBeInteracted={canBeInteracted}, isResetting={isResetting}, minigameRunning={minigameRunning}");
            return;
        }

        if (other.CompareTag("NPC"))
        {
            Debug.Log($"[CoreZone] ✅ {gameObject.name} - NPC entered, starting minigame");

            if (minigamePrefab != null && spawnedMinigame == null)
            {
                // Đánh dấu đang chạy minigame để tránh double trigger
                minigameRunning = true;
                canBeInteracted = false; // Tắt ngay để tránh trigger lại
                
                Debug.Log($"[CoreZone] 🎯 {gameObject.name} - Minigame started! minigameRunning=true, canBeInteracted=false");
                
                // Lấy health fill từ LyraHealth
                GameObject lyraObj = GameObject.FindGameObjectWithTag("NPC");
                if (lyraObj != null)
                {
                    lyraHealth = lyraObj.GetComponent<LyraHealth>();
                    if (lyraHealth != null)
                    {
                        // Đăng ký sự kiện chết
                        lyraHealth.OnDeath += HandleNpcDeath;
                    }
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
                    Debug.Log($"[CoreZone] 🏆 {gameObject.name} - Minigame completed!");
                    minigameRunning = false;
                    // canBeInteracted = false; // Đã set ở trên
                    coreManager.MarkCoreAsComplete(this);
                    ItemsDropManager.Instance.DropItem(rewardCore, 1, this.transform.position + spawnCore);
                    Destroy(spawnedMinigame);
                    spawnedMinigame = null;
                    StartCoroutine(OpenDoorSequence());
                };

                // Bắt đầu minigame
                minigame.StartMinigame();
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("NPC"))
        {
            hasNpcInside = false;
            Debug.Log($"[CoreZone] {gameObject.name} - NPC exited zone");
        }
    }

    private void HandleNpcDeath()
    {
        if (minigameRunning)
        {
            Debug.Log("NPC chết → thua game!");
            minigameRunning = false;
            Destroy(spawnedMinigame);
            if (spawnedMinigame != null)
            {
                var minigame = spawnedMinigame.GetComponent<CoreMinigameController>();
                if (minigame != null)
                {
                    minigame.StopMinigame();
                }
            }
            spawnedMinigame = null;
            
            // Reset lại khả năng interact nếu NPC chết
            if (hasNpcInside && coreManager != null)
            {
                // Kiểm tra xem core này có phải đang active không
                int coreIndex = coreManager.cores.IndexOf(this);
                if (coreIndex >= 0 && coreIndex == coreManager.GetCurrentCoreIndex())
                {
                    canBeInteracted = true;
                }
            }

            if (lyraHealth != null)
                lyraHealth.OnDeath -= HandleNpcDeath; // Hủy đăng ký event
        }
    }

    public void SetActiveLogic(bool active)
    {
        Debug.Log($"[CoreZone] {gameObject.name} - SetActiveLogic: {active}, isResetting: {isResetting}");
        
        if (isResetting)
        {
            Debug.Log($"[CoreZone] {gameObject.name} - Skipping SetActiveLogic because is resetting");
            return;
        }
        
        canBeInteracted = active;
        
        // QUAN TRỌNG: Nếu NPC đang trong zone và được set active, delay một chút
        if (active && hasNpcInside)
        {
            Debug.Log($"[CoreZone] {gameObject.name} - NPC is inside, delaying activation");
            StartCoroutine(DelayedActivation());
        }
    }
    
    // Coroutine để delay activation khi NPC đang trong zone
    private IEnumerator DelayedActivation()
    {
        canBeInteracted = false; // Tắt tạm thời
        yield return new WaitForSeconds(0.5f); // Đợi 0.5 giây
        
        if (!isResetting && !minigameRunning)
        {
            canBeInteracted = true;
            Debug.Log($"[CoreZone] {gameObject.name} - Delayed activation completed");
        }
    }

    // Method mới để reset CoreZone về trạng thái ban đầu cho mini game
    public void ResetCoreForMiniGame()
    {
        Debug.Log($"[CoreZone] Resetting core: {gameObject.name} - Current canBeInteracted: {canBeInteracted}");

        // QUAN TRỌNG: Đánh dấu đang reset để block mọi trigger
        isResetting = true;
        canBeInteracted = false;

        // Hủy minigame đang chạy nếu có
        if (spawnedMinigame != null)
        {
            Debug.Log($"[CoreZone] Destroying existing minigame for {gameObject.name}");
            Destroy(spawnedMinigame);
            spawnedMinigame = null;
        }

        // Reset trạng thái
        minigameRunning = false;

        // QUAN TRỌNG: Hủy đăng ký event trước khi reset
        if (lyraHealth != null)
        {
            lyraHealth.OnDeath -= HandleNpcDeath;
            lyraHealth = null;
            Debug.Log($"[CoreZone] Unregistered health event for {gameObject.name}");
        }

        // Reset collider về trạng thái ban đầu (đóng cửa)
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (var col in colliders)
        {
            if (!col.isTrigger) // collider chặn đường
            {
                col.enabled = initialColliderState; // Trở về trạng thái ban đầu (thường là true - cửa đóng)
                Debug.Log($"[CoreZone] Reset collider for {gameObject.name} to {initialColliderState}");
            }
        }

        // QUAN TRỌNG: Reset animation về trạng thái ban đầu (đóng cửa)
        Animator doorAnimator = GetComponent<Animator>();
        if (doorAnimator != null)
        {
            // Reset về trạng thái ban đầu
            doorAnimator.Play("ClosedState", 0, 0f); // Giả sử có animation state "ClosedState"
            // Hoặc nếu có trigger để đóng cửa:
            // doorAnimator.SetTrigger("Close");
            Debug.Log($"[CoreZone] Reset animator for {gameObject.name}");
        }

        // Delay việc kết thúc reset để đảm bảo NPC được move về vị trí mới
        StartCoroutine(FinishReset());
    }
    
    // Coroutine để kết thúc quá trình reset
    private IEnumerator FinishReset()
    {
        yield return new WaitForSeconds(1f); // Đợi 1 giây để NPC được reset vị trí
        
        isResetting = false;
        Debug.Log($"[CoreZone] Core {gameObject.name} reset completed - ready for CoreManager to set active");
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
}