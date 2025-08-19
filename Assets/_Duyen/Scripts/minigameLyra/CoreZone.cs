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

    private bool canBeInteracted = false;
    private CoreManager coreManager;
    private GameObject spawnedMinigame;

    private bool minigameRunning = false;
    private LyraHealth lyraHealth;

    // Lưu trữ trạng thái ban đầu
    private bool initialCanBeInteracted = false;
    private bool initialColliderState = true;
    
    // QUAN TRỌNG: Thêm flag để prevent auto-trigger sau restart
    private bool isResetting = false;
    private bool hasNpcInside = false;
    
    // QUAN TRỌNG: Thêm trạng thái completion
    private bool isCompleted = false;

    void Start()
    {
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
            Debug.Log($"[CoreZone] 🚀 {gameObject.name} - NPC entered zone. canBeInteracted={canBeInteracted}, isResetting={isResetting}, minigameRunning={minigameRunning}, isCompleted={isCompleted}");
        }
        
        // QUAN TRỌNG: Chặn trigger nếu đã hoàn thành, đang reset, hoặc không thể interact
        if (!canBeInteracted || isResetting || minigameRunning || isCompleted)
        {
            Debug.Log($"[CoreZone] ❌ {gameObject.name} - BLOCKED: canBeInteracted={canBeInteracted}, isResetting={isResetting}, minigameRunning={minigameRunning}, isCompleted={isCompleted}");
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
                    isCompleted = true; // QUAN TRỌNG: Đánh dấu đã hoàn thành
                    
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
            
            // Reset lại khả năng interact nếu NPC chết (nhưng chỉ khi chưa hoàn thành)
            if (hasNpcInside && coreManager != null && !isCompleted)
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
        Debug.Log($"[CoreZone] {gameObject.name} - SetActiveLogic: {active}, isResetting: {isResetting}, isCompleted: {isCompleted}");
        
        if (isResetting)
        {
            Debug.Log($"[CoreZone] {gameObject.name} - Skipping SetActiveLogic because is resetting");
            return;
        }
        
        // QUAN TRỌNG: Không cho phép set active nếu đã hoàn thành
        if (isCompleted)
        {
            Debug.Log($"[CoreZone] {gameObject.name} - Skipping SetActiveLogic because already completed");
            canBeInteracted = false;
            return;
        }
        
        canBeInteracted = active;
        
        // Nếu NPC đang trong zone và được set active, delay một chút
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
        
        if (!isResetting && !minigameRunning && !isCompleted)
        {
            canBeInteracted = true;
            Debug.Log($"[CoreZone] {gameObject.name} - Delayed activation completed");
        }
    }

    // Method để set core as completed (gọi từ CoreManager khi restore progress)
    public void SetAsCompleted()
    {
        Debug.Log($"[CoreZone] {gameObject.name} - Setting as completed");
        
        isCompleted = true;
        canBeInteracted = false;
        minigameRunning = false;
        
        // Mở cửa ngay lập tức
        StartCoroutine(OpenDoorSequence());
    }

    // Method để reset CoreZone về trạng thái ban đầu cho mini game
    public void ResetCoreForMiniGame()
    {
        Debug.Log($"[CoreZone] Resetting core: {gameObject.name} - Current canBeInteracted: {canBeInteracted}, isCompleted: {isCompleted}");

        // QUAN TRỌNG: Đánh dấu đang reset để block mọi trigger
        isResetting = true;
        canBeInteracted = false;

        // QUAN TRỌNG: Chỉ reset completion nếu đây là full reset, không phải preserve progress
        // (completion sẽ được restore lại trong CoreManager nếu cần)

        // Hủy minigame đang chạy nếu có
        if (spawnedMinigame != null)
        {
            Debug.Log($"[CoreZone] Destroying existing minigame for {gameObject.name}");
            Destroy(spawnedMinigame);
            spawnedMinigame = null;
        }

        // Reset trạng thái
        minigameRunning = false;

        // Hủy đăng ký event trước khi reset
        if (lyraHealth != null)
        {
            lyraHealth.OnDeath -= HandleNpcDeath;
            lyraHealth = null;
            Debug.Log($"[CoreZone] Unregistered health event for {gameObject.name}");
        }

        // QUAN TRỌNG: Chỉ reset physical state nếu chưa hoàn thành
        if (!isCompleted)
        {
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

            // Reset animation về trạng thái ban đầu (đóng cửa)
            Animator doorAnimator = GetComponent<Animator>();
            if (doorAnimator != null)
            {
                doorAnimator.Play("ClosedState", 0, 0f);
                Debug.Log($"[CoreZone] Reset animator for {gameObject.name}");
            }
        }
        else
        {
            Debug.Log($"[CoreZone] {gameObject.name} is completed - keeping door open");
        }

        // Delay việc kết thúc reset
        StartCoroutine(FinishReset());
    }
    
    // Method để reset completion status (chỉ gọi khi full reset)
    public void ResetCompletion()
    {
        Debug.Log($"[CoreZone] {gameObject.name} - Resetting completion status");
        isCompleted = false;
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

        // Chạy animation mở cửa
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
    
    // Method để check trạng thái completion
    public bool IsCompleted()
    {
        return isCompleted;
    }
}