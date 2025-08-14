using UnityEngine;
using System.Collections;

public class Boss2DeathHandler : MonoBehaviour
{
    [Header("Boss Settings")]
    [SerializeField] private Boss2Controller boss2Controller; // Reference trực tiếp
    [SerializeField] private string boss2Name = "Boss2"; // Tên boss để tìm
    [SerializeField] private float checkInterval = 0.5f; // Tần suất check boss
    
    [Header("Door Settings")]
    [SerializeField] private GameObject door;
    [SerializeField] private float doorActivationDelay = 1f;
    
    [Header("Camera Shake Settings")]
    [SerializeField] private float shakeIntensity = 2f;
    [SerializeField] private float shakeDuration = 3f;
    [SerializeField] private float shakeFrequency = 0.1f;
    
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip doorOpenSound;
    [SerializeField] private AudioClip victorySound;
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject doorOpenEffect;
    [SerializeField] private Light doorLight;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    
    private bool hasTriggered = false;
    private bool isCheckingBoss = false;
    private Coroutine bossCheckCoroutine;
    
    void Start()
    {
        // Đảm bảo cửa bị ẩn ban đầu
        if (door != null)
        {
            door.SetActive(false);
        }
        
        // Setup audio source
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Bắt đầu check boss
        StartBossChecking();
    }
    
    private void StartBossChecking()
    {
        if (!isCheckingBoss)
        {
            isCheckingBoss = true;
            bossCheckCoroutine = StartCoroutine(CheckForBossCoroutine());
            
            if (enableDebugLogs)
                Debug.Log("Bắt đầu tìm kiếm Boss2...");
        }
    }
    
    private void StopBossChecking()
    {
        if (isCheckingBoss)
        {
            isCheckingBoss = false;
            if (bossCheckCoroutine != null)
            {
                StopCoroutine(bossCheckCoroutine);
                bossCheckCoroutine = null;
            }
        }
    }
    
    private IEnumerator CheckForBossCoroutine()
    {
        while (isCheckingBoss && !hasTriggered)
        {
            // Tìm boss2 nếu chưa có reference
            if (boss2Controller == null)
            {
                FindBoss2();
            }
            
            // Nếu đã tìm thấy boss và boss đã active
            if (boss2Controller != null && boss2Controller.gameObject.activeInHierarchy)
            {
                if (enableDebugLogs)
                    Debug.Log("Đã tìm thấy Boss2 và boss đã active! Bắt đầu theo dõi...");
                
                // Chuyển sang mode theo dõi boss
                StopBossChecking();
                StartCoroutine(MonitorBossHealth());
                yield break;
            }
            
            yield return new WaitForSeconds(checkInterval);
        }
    }
    
    private void FindBoss2()
    {
        // Tìm theo reference trực tiếp trước
        if (boss2Controller == null)
        {
            // Tìm theo tên
            GameObject bossObject = GameObject.Find(boss2Name);
            if (bossObject != null)
            {
                boss2Controller = bossObject.GetComponent<Boss2Controller>();
            }
            
            // Nếu vẫn không tìm thấy, tìm theo type
            if (boss2Controller == null)
            {
                boss2Controller = FindObjectOfType<Boss2Controller>();
            }
        }
    }
    
    private IEnumerator MonitorBossHealth()
    {
        if (enableDebugLogs)
            Debug.Log("Bắt đầu theo dõi máu Boss2...");
        
        while (!hasTriggered && boss2Controller != null)
        {
            // Kiểm tra boss có còn active không
            if (!boss2Controller.gameObject.activeInHierarchy)
            {
                if (enableDebugLogs)
                    Debug.Log("Boss2 đã bị deactive, quay lại tìm kiếm...");
                
                boss2Controller = null;
                StartBossChecking();
                yield break;
            }
            
            // Kiểm tra boss đã chết chưa
            if (IsBoss2Dead())
            {
                hasTriggered = true;
                StartCoroutine(HandleBoss2Death());
                yield break;
            }
            
            yield return new WaitForSeconds(0.1f); // Check thường xuyên hơn khi boss active
        }
    }
    
    private bool IsBoss2Dead()
    {
        if (boss2Controller == null) return false;
        
        // Kiểm tra qua damageReceiver trước
        if (boss2Controller.damageReceiver != null)
        {
            return boss2Controller.damageReceiver.IsDead();
        }
        
        // Backup: kiểm tra qua enabled state
        return !boss2Controller.enabled;
    }
    
    private IEnumerator HandleBoss2Death()
    {
        if (enableDebugLogs)
            Debug.Log("Boss2 đã chết! Bắt đầu xử lý...");
        
        // 1. Camera Shake ngay lập tức
        StartCameraShake();
        
        // 2. Phát âm thanh victory (nếu có)
        if (victorySound != null)
        {
            audioSource.PlayOneShot(victorySound);
        }
        
        // 3. Đợi một chút trước khi mở cửa
        yield return new WaitForSeconds(doorActivationDelay);
        
        // 4. Mở cửa
        OpenDoor();
    }
    
    private void StartCameraShake()
    {
        if (CameraFollow.Instance != null)
        {
            StartCoroutine(ExtendedCameraShake());
        }
        else
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                StartCoroutine(ManualCameraShake(mainCamera));
            }
        }
    }
    
    private IEnumerator ExtendedCameraShake()
    {
        float elapsed = 0f;
        
        while (elapsed < shakeDuration)
        {
            float currentIntensity = shakeIntensity * (1f - elapsed / shakeDuration);
            CameraFollow.Instance.ShakeCamera();
            
            yield return new WaitForSeconds(shakeFrequency);
            elapsed += shakeFrequency;
        }
    }
    
    private IEnumerator ManualCameraShake(Camera camera)
    {
        Vector3 originalPosition = camera.transform.position;
        float elapsed = 0f;
        
        while (elapsed < shakeDuration)
        {
            float currentIntensity = shakeIntensity * (1f - elapsed / shakeDuration);
            
            float offsetX = Random.Range(-currentIntensity, currentIntensity);
            float offsetY = Random.Range(-currentIntensity, currentIntensity);
            
            camera.transform.position = originalPosition + new Vector3(offsetX, offsetY, 0);
            
            yield return new WaitForSeconds(shakeFrequency);
            elapsed += shakeFrequency;
        }
        
        camera.transform.position = originalPosition;
    }
    
    private void OpenDoor()
    {
        if (door != null)
        {
            door.SetActive(true);
            
            if (doorOpenSound != null)
            {
                audioSource.PlayOneShot(doorOpenSound);
            }
            
            if (doorOpenEffect != null)
            {
                doorOpenEffect.SetActive(true);
            }
            
            if (doorLight != null)
            {
                StartCoroutine(FadeLightIn());
            }
            
            if (enableDebugLogs)
                Debug.Log("Cửa đã được mở!");
        }
        else
        {
            Debug.LogWarning("Door GameObject chưa được assign!");
        }
    }
    
    private IEnumerator FadeLightIn()
    {
        float duration = 2f;
        float elapsed = 0f;
        float targetIntensity = doorLight.intensity;
        
        doorLight.intensity = 0f;
        
        while (elapsed < duration)
        {
            doorLight.intensity = Mathf.Lerp(0f, targetIntensity, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        doorLight.intensity = targetIntensity;
    }
    
    // Public methods
    public void ForceOpenDoor()
    {
        if (!hasTriggered)
        {
            hasTriggered = true;
            StopBossChecking();
            StartCoroutine(HandleBoss2Death());
        }
    }
    
    public void ResetHandler()
    {
        hasTriggered = false;
        StopBossChecking();
        boss2Controller = null;
        
        if (door != null)
        {
            door.SetActive(false);
        }
        
        if (doorOpenEffect != null)
        {
            doorOpenEffect.SetActive(false);
        }
        
        if (doorLight != null)
        {
            doorLight.intensity = 0f;
        }
        
        StartBossChecking();
    }
    
    // Gọi method này khi boss2 được active (từ Combat Trigger)
    public void OnBoss2Activated(Boss2Controller boss)
    {
        if (boss != null)
        {
            boss2Controller = boss;
            if (enableDebugLogs)
                Debug.Log("Boss2 đã được activate thông qua callback!");
        }
    }
    
    void OnDestroy()
    {
        StopBossChecking();
    }
}