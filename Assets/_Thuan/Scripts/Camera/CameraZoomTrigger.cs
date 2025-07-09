using UnityEngine;
using System.Collections;

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
            
            // Play zoom sound
            if (zoomSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(zoomSound);
            }
            
            // Zoom camera
            StartCoroutine(ZoomCamera(targetZoomSize));
        }
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
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / zoomDuration;
            
            // Smooth zoom transition
            float currentSize = Mathf.Lerp(startSize, targetSize, progress);
            mainCam.orthographicSize = currentSize;
            
            // Notify CameraBoundaries to update
            CameraBoundaries boundaries = FindObjectOfType<CameraBoundaries>();
            if (boundaries != null)
            {
                boundaries.UpdateCameraSize();
            }
            
            yield return null;
        }
        
        // Ensure final size is exact
        mainCam.orthographicSize = targetSize;
    }
    
    // Method để reset trigger từ bên ngoài
    public void ResetTrigger()
    {
        hasTriggered = false;
    }
    
    // Method để force zoom
    public void ForceZoom(float size)
    {
        StartCoroutine(ZoomCamera(size));
    }
}