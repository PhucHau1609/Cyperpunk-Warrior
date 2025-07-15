using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;

public class CameraZoomTrigger : MonoBehaviour
{
    [Header("Zoom Settings")]
    public float targetZoomSize = 10f;
    public float zoomDuration = 2f;
    public bool zoomOnEnter = true;
    public bool zoomOnExit = false;
    
    [Header("Audio Settings")]
    public AudioClip zoomSound;
    [Range(0f, 1f)]
    public float audioVolume = 0.5f;
    
    private float originalZoomSize;
    private bool hasTriggered = false;
    private AudioSource audioSource;

    [Header("Boss Control")]
    public MonoBehaviour bossController; 
    public bool disableBossOnTrigger = true;
    public BehaviorTree behaviorTree;
    public Rigidbody2D rb;

    [Header("Collider Management")]
    [Tooltip("Danh sách các Collider2D sẽ được tắt/bật")]
    public List<Collider2D> collidersToControl = new List<Collider2D>();
    
    [Header("Dialogue")]
    public GameObject dialogueHolder;
    
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
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && zoomOnEnter && !hasTriggered)
        {
            hasTriggered = true;

            // Disable boss controller và colliders trước khi zoom
            if (disableBossOnTrigger)
            {
                DisableBossComponents();
                DisableColliders();
            }
            
            // Play zoom sound
            if (zoomSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(zoomSound);
            }
            
            // Zoom camera và kích hoạt dialogue
            StartCoroutine(ZoomThenDialogue());
        }
    }

    IEnumerator ZoomThenDialogue()
    {
        // Zoom camera trước
        yield return StartCoroutine(ZoomCamera(targetZoomSize));
        
        // Pause game
        Time.timeScale = 0f;
        
        // Kích hoạt dialogue
        if (dialogueHolder != null)
        {
            dialogueHolder.SetActive(true);
            
            // Đợi dialogue kết thúc
            yield return new WaitUntil(() => !dialogueHolder.activeInHierarchy);
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
        
        if (behaviorTree != null)
        {
            behaviorTree.enabled = true;
        }
        
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
        
        Debug.Log("Boss activated!");
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
        Debug.Log($"Disabled {collidersToControl.Count} colliders");
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
        Debug.Log($"Enabled {collidersToControl.Count} colliders");
    }
    
    // Public methods để sử dụng từ bên ngoài
    public void ResetTrigger()
    {
        hasTriggered = false;
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
}