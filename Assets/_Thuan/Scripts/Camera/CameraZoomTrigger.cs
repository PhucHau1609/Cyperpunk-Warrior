using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using TMPro;

public class CameraZoomTrigger : MonoBehaviour
{
    [Header("Zoom Settings")]
    public float targetZoomSize = 10f;
    public float zoomDuration = 2f;
    public bool zoomOnEnter = true;
    public bool zoomOnExit = false;
    
    [Header("Audio Settings")]
    public AudioClip zoomSound;
    public AudioClip warningSFX;
    [Range(0f, 1f)]
    public float audioVolume = 0.5f;
    [Range(0f, 1f)]
    public float warningVolume = 0.7f;
    
    [Header("Warning Effect Settings")]
    public TextMeshProUGUI warningText;
    public float letterAppearDelay = 0.1f; // Thời gian delay giữa các chữ cái xuất hiện
    public float blinkSpeed = 0.3f; // Tốc độ nhấp nháy
    public float disappearDelay = 1f; // Thời gian chờ trước khi biến mất
    public float letterDisappearDelay = 0.05f; // Thời gian delay giữa các chữ cái biến mất
    public Color redColor = Color.red;
    public Color blackColor = Color.black;
    
    private float originalZoomSize;
    private bool hasTriggered = false;
    private AudioSource audioSource;
    private AudioSource warningAudioSource;
    private string originalWarningText;

    [Header("Boss Control")]
    public MonoBehaviour bossController; 
    public BossPhuDamageReceiver bossPhuDamageReceiver;
    public MiniBoss miniBoss;
    public MiniBossDamageReceiver miniBossDamageReceiver;
    public bool disableBossOnTrigger = true;
    public BehaviorTree behaviorTree;
    public Rigidbody2D rb;

    [Header("Collider Management")]
    [Tooltip("Danh sách các Collider2D sẽ được tắt/bật")]
    public List<Collider2D> collidersToControl = new List<Collider2D>();
    
    [Header("Dialogue")]
    public GameObject dialogueHolder;
    public GameObject miniGame;

    public GameObject[] enemies;
    
    [Header("First Time Trigger Settings")]
    [Tooltip("Có phải lần đầu tiên trigger được kích hoạt không")]
    public bool isFirstTimeTrigger = true;
    
    void Start()
    {
        // Lấy camera size ban đầu
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            originalZoomSize = mainCam.orthographicSize;
        }
        
        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.volume = audioVolume;
        audioSource.playOnAwake = false;
        
        // Setup warning audio source
        warningAudioSource = gameObject.AddComponent<AudioSource>();
        warningAudioSource.volume = warningVolume;
        warningAudioSource.playOnAwake = false;
        warningAudioSource.loop = false;
        
        // Setup warning text
        if (warningText != null)
        {
            originalWarningText = warningText.text;
            warningText.text = "";
            warningText.gameObject.SetActive(false);
        }
        
        // Nếu không phải lần đầu (respawn), disable boss components ngay từ đầu
        if (!isFirstTimeTrigger)
        {
            DisableBossComponents();
            DisableColliders();
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && zoomOnEnter && !hasTriggered)
        {
            hasTriggered = true;

            if (isFirstTimeTrigger)
            {
                // Lần đầu tiên: Disable boss -> Warning -> Dialogue -> Enable boss
                if (disableBossOnTrigger)
                {
                    DisableBossComponents();
                    DisableColliders();
                }
                StartCoroutine(FirstTimeSequence());
            }
            else
            {
                // Lần sau (respawn): Chỉ chạy Warning -> Enable boss (không có dialogue)
                StartCoroutine(RespawnSequence());
            }
        }
    }

    // Sequence cho lần đầu tiên (có dialogue)
    IEnumerator FirstTimeSequence()
    {
        // Bắt đầu với hiệu ứng Warning
        yield return StartCoroutine(WarningAndZoomSequence());
        
        // Tiếp tục với dialogue
        yield return StartCoroutine(DialogueSequence());
        
        // Đánh dấu không còn là lần đầu nữa
        isFirstTimeTrigger = false;
    }
    
    // Sequence cho lần respawn (không có dialogue)
    IEnumerator RespawnSequence()
    {
        // Chỉ chạy Warning + Zoom
        yield return StartCoroutine(WarningAndZoomSequence());
        
        // Kích hoạt boss ngay sau Warning
        EnableBossComponents();
        EnableColliders();
        
        // Tắt trigger này
        gameObject.SetActive(false);
    }

    IEnumerator WarningAndZoomSequence()
    {
        // Bắt đầu zoom và warning đồng thời
        Coroutine zoomCoroutine = null;
        Coroutine warningCoroutine = null;
        
        // Play zoom sound trước
        if (zoomSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(zoomSound);
        }
        
        // Bắt đầu zoom camera
        zoomCoroutine = StartCoroutine(ZoomCamera(targetZoomSize));
        
        // Bắt đầu hiệu ứng Warning
        if (warningText != null)
        {
            warningCoroutine = StartCoroutine(WarningEffect());
        }
        
        // Đợi cả zoom và warning hoàn thành
        if (zoomCoroutine != null)
        {
            yield return zoomCoroutine;
        }
        
        if (warningCoroutine != null)
        {
            yield return warningCoroutine;
        }
    }
    
    IEnumerator WarningEffect()
    {
        // Phát SFX Warning
        if (warningSFX != null && warningAudioSource != null)
        {
            warningAudioSource.clip = warningSFX;
            warningAudioSource.Play();
        }
        
        warningText.gameObject.SetActive(true);
        
        // Hiệu ứng xuất hiện từng chữ cái
        yield return StartCoroutine(ShowWarningLetters());
        
        // Chờ 1 giây rồi biến mất
        yield return new WaitForSeconds(disappearDelay);
        
        // Hiệu ứng biến mất từng chữ cái
        yield return StartCoroutine(HideWarningLetters());
        
        // Tắt SFX Warning khi hiệu ứng kết thúc
        if (warningAudioSource != null && warningAudioSource.isPlaying)
        {
            warningAudioSource.Stop();
        }
        
        // Ẩn warning text
        warningText.gameObject.SetActive(false);
    }

    IEnumerator ShowWarningLetters()
    {
        if (warningText == null || string.IsNullOrEmpty(originalWarningText)) yield break;
        
        warningText.text = "";
        warningText.color = redColor;
        
        // Coroutine cho hiệu ứng nhấp nháy
        Coroutine blinkCoroutine = StartCoroutine(BlinkWarningText());
        
        // Hiển thị từng chữ cái
        for (int i = 0; i <= originalWarningText.Length; i++)
        {
            warningText.text = originalWarningText.Substring(0, i);
            yield return new WaitForSeconds(letterAppearDelay);
        }
        
        // Dừng hiệu ứng nhấp nháy
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }
        
        // Đảm bảo text hiển thị đầy đủ với màu đỏ
        warningText.text = originalWarningText;
        warningText.color = redColor;
    }
    
    IEnumerator HideWarningLetters()
    {
        if (warningText == null || string.IsNullOrEmpty(originalWarningText)) yield break;
        
        // Bắt đầu hiệu ứng nhấp nháy lại
        Coroutine blinkCoroutine = StartCoroutine(BlinkWarningText());
        
        // Ẩn từng chữ cái (từ cuối về đầu)
        for (int i = originalWarningText.Length; i >= 0; i--)
        {
            warningText.text = originalWarningText.Substring(0, i);
            yield return new WaitForSeconds(letterDisappearDelay);
        }
        
        // Dừng hiệu ứng nhấp nháy
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }
        
        warningText.text = "";
    }
    
    IEnumerator BlinkWarningText()
    {
        if (warningText == null) yield break;
        
        bool useRedColor = true;
        
        while (true)
        {
            warningText.color = useRedColor ? redColor : blackColor;
            useRedColor = !useRedColor;
            yield return new WaitForSeconds(blinkSpeed);
        }
    }

    IEnumerator DialogueSequence()
    {
        // Pause game
        Time.timeScale = 0f;
        
        // Kích hoạt dialogue
        if (dialogueHolder != null)
        {
            dialogueHolder.SetActive(true);
            
            // Đợi dialogue kết thúc
            yield return new WaitUntil(() => !dialogueHolder.activeInHierarchy);
        }
        
        if (miniGame != null)
        {
            miniGame.SetActive(true);
            
            // Đợi minigame kết thúc
            yield return new WaitUntil(() => !miniGame.activeInHierarchy);
        }

        if (enemies != null)
        {
            foreach (GameObject enemy in enemies)
            {
                if (enemy != null)
                    enemy.SetActive(true);
            }
        }

        // Resume game
        Time.timeScale = 1f;
        
        // Kích hoạt boss controller và colliders SAU KHI dialogue xong
        EnableBossComponents();
        EnableColliders();
        
        // Tắt trigger này
        gameObject.SetActive(false);
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && zoomOnExit && hasTriggered)
        {
            hasTriggered = false;
            
            // Play zoom sound
            if (zoomSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(zoomSound);
            }
            
            // Zoom back to original
            StartCoroutine(ZoomCamera(originalZoomSize));
        }
    }
    
    IEnumerator ZoomCamera(float targetSize)
    {
        Camera mainCam = Camera.main;
        if (mainCam == null) yield break;
        
        float startSize = mainCam.orthographicSize;
        float elapsedTime = 0f;
        
        while (elapsedTime < zoomDuration)
        {
            elapsedTime += Time.unscaledDeltaTime; // Dùng unscaledDeltaTime để không bị ảnh hưởng bởi timeScale
            float progress = elapsedTime / zoomDuration;
            
            // Smooth zoom transition
            float currentSize = Mathf.Lerp(startSize, targetSize, progress);
            mainCam.orthographicSize = currentSize;
            
            // Notify CameraBoundaries to update
            CameraBoundaries boundaries = FindFirstObjectByType<CameraBoundaries>();
            if (boundaries != null)
            {
                boundaries.UpdateCameraSize();
            }
            
            yield return null;
        }
        
        // Ensure final size is exact
        mainCam.orthographicSize = targetSize;
    }
    
    // Methods để quản lý boss components
    private void DisableBossComponents()
    {
        if (bossController != null)
        {
            bossController.enabled = false;
        }

        if (miniBoss != null)
        {
            miniBoss.enabled = false;
        }

        if (bossPhuDamageReceiver != null)
        {
            bossPhuDamageReceiver.enabled = false;
        }
        
        if (miniBossDamageReceiver != null)
        {
            miniBossDamageReceiver.enabled = false;
        }
        
        if (behaviorTree != null)
        {
            behaviorTree.enabled = false;
        }
        
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Static;
        }
    }
    
    private void EnableBossComponents()
    {
        if (bossController != null)
        {
            bossController.enabled = true;
        }
        
        if (miniBoss != null)
        {
            miniBoss.enabled = true;
        }

        if (bossPhuDamageReceiver != null)
        {
            bossPhuDamageReceiver.enabled = true;
        }
        
        if (miniBossDamageReceiver != null)
        {
            miniBossDamageReceiver.enabled = true;
        }

        if (behaviorTree != null)
        {
            behaviorTree.enabled = true;
        }
        
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
    }
    
    // Methods để quản lý colliders
    private void DisableColliders()
    {
        foreach (Collider2D col in collidersToControl)
        {
            if (col != null)
            {
                col.enabled = false;
            }
        }
    }
    
    private void EnableColliders()
    {
        foreach (Collider2D col in collidersToControl)
        {
            if (col != null)
            {
                col.enabled = true;
            }
        }
    }
    
    // Public methods để sử dụng từ bên ngoài
    public void ResetTrigger()
    {
        hasTriggered = false;
        // Reset về trạng thái lần đầu sau khi respawn
        isFirstTimeTrigger = false;
        
        // Khi reset trigger, disable boss components
        if (disableBossOnTrigger)
        {
            DisableBossComponents();
            DisableColliders();
        }
        
        // KHÔNG tắt trigger ở đây nữa, để CheckpointManager quản lý
        // gameObject.SetActive(true); // Đã được handle bởi CheckpointManager
    }
    
    public void ForceZoom(float size)
    {
        StartCoroutine(ZoomCamera(size));
    }
    
    // Methods để thêm/xóa colliders động
    public void AddCollider(Collider2D collider)
    {
        if (collider != null && !collidersToControl.Contains(collider))
        {
            collidersToControl.Add(collider);
        }
    }
    
    public void RemoveCollider(Collider2D collider)
    {
        if (collider != null && collidersToControl.Contains(collider))
        {
            collidersToControl.Remove(collider);
        }
    }
    
    public void ClearColliderList()
    {
        collidersToControl.Clear();
    }
    
    // Method để force disable/enable colliders từ bên ngoài
    public void ForceDisableColliders()
    {
        DisableColliders();
    }
    
    public void ForceEnableColliders()
    {
        EnableColliders();
    }
    
    // Public methods để điều khiển warning effect từ bên ngoài
    public void PlayWarningEffect()
    {
        if (warningText != null)
        {
            StartCoroutine(WarningAndZoomSequence());
        }
    }
    
    public void SetWarningText(string newText)
    {
        originalWarningText = newText;
    }
    
    public void StopWarningSFX()
    {
        if (warningAudioSource != null && warningAudioSource.isPlaying)
        {
            warningAudioSource.Stop();
        }
    }
    
    // Method để set trạng thái first time từ bên ngoài
    public void SetFirstTimeTrigger(bool isFirstTime)
    {
        isFirstTimeTrigger = isFirstTime;
    }
}