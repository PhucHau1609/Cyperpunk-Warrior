using UnityEngine;
using System.Collections;
using TMPro;
using System.Collections.Generic;

public class BossWarningManager : MonoBehaviour
{
    [System.Serializable]
    public class BossWarningData
    {
        public string bossName;
        public MiniBoss miniBoss;
        public MiniBossDamageReceiver damageReceiver;
        public Rigidbody2D rb;
        public List<Collider2D> collidersToControl = new List<Collider2D>();
        public bool hasBeenTriggeredBefore = false;
    }
    
    [Header("Warning Effect Settings")]
    public TextMeshProUGUI warningText;
    public string warningMessage = "WARNING";
    public float letterAppearDelay = 0.1f;
    public float blinkSpeed = 0.3f;
    public float disappearDelay = 1f;
    public float letterDisappearDelay = 0.05f;
    public Color redColor = Color.red;
    public Color blackColor = Color.black;
    
    [Header("Camera Zoom Settings")]
    public float targetZoomSize = 10f;
    public float zoomDuration = 2f;
    
    [Header("Audio Settings")]
    public AudioClip warningSFX;
    public AudioClip zoomSound;
    [Range(0f, 1f)]
    public float warningVolume = 0.7f;
    [Range(0f, 1f)]
    public float zoomVolume = 0.5f;
    
    [Header("Boss Data")]
    public List<BossWarningData> bossDataList = new List<BossWarningData>();
    
    [Header("First Time Settings")]
    public GameObject dialogueHolder;
    public GameObject miniGame;
    public GameObject[] enemiesToActivate;
    
    private AudioSource warningAudioSource;
    private AudioSource zoomAudioSource;
    private float originalZoomSize;
    private string originalWarningText;
    
    // Singleton pattern
    public static BossWarningManager Instance;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        // Get original camera size
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            originalZoomSize = mainCam.orthographicSize;
        }
        
        // Setup audio sources
        SetupAudioSources();
        
        // Setup warning text
        SetupWarningText();
        
        // Initially disable all bosses
        DisableAllBosses();
    }
    
    private void SetupAudioSources()
    {
        // Warning audio source
        warningAudioSource = gameObject.AddComponent<AudioSource>();
        warningAudioSource.volume = warningVolume;
        warningAudioSource.playOnAwake = false;
        warningAudioSource.loop = false;
        
        // Zoom audio source
        zoomAudioSource = gameObject.AddComponent<AudioSource>();
        zoomAudioSource.volume = zoomVolume;
        zoomAudioSource.playOnAwake = false;
        zoomAudioSource.loop = false;
    }
    
    private void SetupWarningText()
    {
        if (warningText != null)
        {
            originalWarningText = string.IsNullOrEmpty(warningMessage) ? warningText.text : warningMessage;
            warningText.text = "";
            warningText.gameObject.SetActive(false);
        }
    }
    
    // Method được gọi từ BossCombatZone khi player vào zone sau respawn
    public void TriggerBossWarningOnRespawnInZone(BossCombatZone combatZone)
    {
        currentCombatZone = combatZone;
        StartCoroutine(RespawnWarningSequenceInZone());
    }
    
    [Header("Combat Zone Integration")]
    public BossCombatZone currentCombatZone;
    
    // Method được gọi từ BossCombatZone khi player vào zone lần đầu
    public void TriggerBossWarningForZone(BossCombatZone combatZone)
    {
        currentCombatZone = combatZone;
        StartCoroutine(FirstTimeWarningSequenceForZone());
    }
    
    // Method được gọi từ BossZoneTrigger lần đầu tiên (để tương thích ngược)
    public void TriggerBossWarningFirstTime()
    {
        StartCoroutine(FirstTimeWarningSequence());
    }
    
    private IEnumerator FirstTimeWarningSequence()
    {
        Debug.Log("[BossWarningManager] Starting first time warning sequence");
        
        // Mark all bosses as triggered before
        foreach (var bossData in bossDataList)
        {
            bossData.hasBeenTriggeredBefore = true;
        }
        
        // Disable all bosses first
        DisableAllBosses();
        
        // Run warning + zoom
        yield return StartCoroutine(WarningAndZoomSequence());
        
        // Show dialogue
        yield return StartCoroutine(DialogueSequence());
        
        // Enable bosses after dialogue
        EnableAllBosses();
    }
    
    // Method được gọi từ CheckpointManager khi player respawn
    public void TriggerBossWarningOnRespawn()
    {
        StartCoroutine(RespawnWarningSequence());
    }
    
    private IEnumerator RespawnWarningSequenceInZone()
    {
        Debug.Log("[BossWarningManager] Starting respawn warning for zone entry");
        
        // Run warning + zoom (no dialogue)
        yield return StartCoroutine(WarningAndZoomSequence());
        
        // Activate bosses or mini game in the current zone
        if (currentCombatZone != null)
        {
            // QUAN TRỌNG: Kiểm tra nếu là mini game zone
            if (currentCombatZone.isMiniGameZone)
            {
                TriggerMiniGameInZone();
            }
            else
            {
                currentCombatZone.ActivateBosses();
            }
        }
        
        Debug.Log("[BossWarningManager] Respawn zone warning sequence completed");
    }
    
    private IEnumerator FirstTimeWarningSequenceForZone()
    {
        Debug.Log("[BossWarningManager] Starting first time warning for zone");
        
        // Run warning + zoom
        yield return StartCoroutine(WarningAndZoomSequence());
        
        // Show dialogue
        yield return StartCoroutine(DialogueSequence());
        
        // Activate bosses or mini game in the current zone
        if (currentCombatZone != null)
        {
            // QUAN TRỌNG: Kiểm tra nếu là mini game zone
            if (currentCombatZone.isMiniGameZone)
            {
                TriggerMiniGameInZone();
            }
            else
            {
                currentCombatZone.ActivateBosses();
            }
        }
        else
        {
            // Fallback: Enable all bosses in list
            EnableAllBosses();
        }
    }
    
    // Method mới để trigger mini game trong zone
    private void TriggerMiniGameInZone()
    {
        Debug.Log("[BossWarningManager] Triggering mini game in zone");
        
        SceneController sceneController = FindFirstObjectByType<SceneController>();
        if (sceneController != null)
        {
            sceneController.SwitchToPetControl();
            Debug.Log("[BossWarningManager] Mini game activated via SceneController");
        }
        else
        {
            Debug.LogWarning("[BossWarningManager] SceneController not found for mini game activation");
        }
    }
    
    private IEnumerator RespawnWarningSequence()
    {
        Debug.Log("[BossWarningManager] Starting respawn warning sequence");
        
        // Run warning + zoom
        yield return StartCoroutine(WarningAndZoomSequence());
        
        // Note: Don't auto-activate bosses here, let Combat Zones handle activation
        // when player enters their zones
        
        Debug.Log("[BossWarningManager] Respawn warning sequence completed");
    }
    
    private IEnumerator WarningAndZoomSequence()
    {
        Coroutine zoomCoroutine = null;
        Coroutine warningCoroutine = null;
        
        // Play zoom sound
        if (zoomSound != null && zoomAudioSource != null)
        {
            zoomAudioSource.PlayOneShot(zoomSound);
        }
        
        // Start zoom camera
        zoomCoroutine = StartCoroutine(ZoomCamera(targetZoomSize));
        
        // Start warning effect
        if (warningText != null)
        {
            warningCoroutine = StartCoroutine(WarningEffect());
        }
        
        // Wait for both to complete
        if (zoomCoroutine != null)
        {
            yield return zoomCoroutine;
        }
        
        if (warningCoroutine != null)
        {
            yield return warningCoroutine;
        }
    }
    
    private IEnumerator WarningEffect()
    {
        // Play warning SFX
        if (warningSFX != null && warningAudioSource != null)
        {
            warningAudioSource.clip = warningSFX;
            warningAudioSource.Play();
        }
        
        warningText.gameObject.SetActive(true);
        
        // Show letters appearing effect
        yield return StartCoroutine(ShowWarningLetters());
        
        // Wait then disappear
        yield return new WaitForSeconds(disappearDelay);
        
        // Hide letters disappearing effect
        yield return StartCoroutine(HideWarningLetters());
        
        // Stop warning SFX
        if (warningAudioSource != null && warningAudioSource.isPlaying)
        {
            warningAudioSource.Stop();
        }
        
        // Hide warning text
        warningText.gameObject.SetActive(false);
    }
    
    private IEnumerator ShowWarningLetters()
    {
        if (warningText == null || string.IsNullOrEmpty(originalWarningText)) yield break;
        
        warningText.text = "";
        warningText.color = redColor;
        
        // Start blinking coroutine
        Coroutine blinkCoroutine = StartCoroutine(BlinkWarningText());
        
        // Show letters one by one
        for (int i = 0; i <= originalWarningText.Length; i++)
        {
            warningText.text = originalWarningText.Substring(0, i);
            yield return new WaitForSeconds(letterAppearDelay);
        }
        
        // Stop blinking
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }
        
        // Ensure full text with red color
        warningText.text = originalWarningText;
        warningText.color = redColor;
    }
    
    private IEnumerator HideWarningLetters()
    {
        if (warningText == null || string.IsNullOrEmpty(originalWarningText)) yield break;
        
        // Start blinking again
        Coroutine blinkCoroutine = StartCoroutine(BlinkWarningText());
        
        // Hide letters from end to start
        for (int i = originalWarningText.Length; i >= 0; i--)
        {
            warningText.text = originalWarningText.Substring(0, i);
            yield return new WaitForSeconds(letterDisappearDelay);
        }
        
        // Stop blinking
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }
        
        warningText.text = "";
    }
    
    private IEnumerator BlinkWarningText()
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
    
    private IEnumerator DialogueSequence()
    {
        // Pause game
        Time.timeScale = 0f;
        
        // Activate dialogue
        if (dialogueHolder != null)
        {
            dialogueHolder.SetActive(true);
            yield return new WaitUntil(() => !dialogueHolder.activeInHierarchy);
        }
        
        if (miniGame != null)
        {
            miniGame.SetActive(true);
            yield return new WaitUntil(() => !miniGame.activeInHierarchy);
        }
        
        if (enemiesToActivate != null)
        {
            foreach (GameObject enemy in enemiesToActivate)
            {
                if (enemy != null)
                    enemy.SetActive(true);
            }
        }
        
        // Resume game
        Time.timeScale = 1f;
    }
    
    private IEnumerator ZoomCamera(float targetSize)
    {
        Camera mainCam = Camera.main;
        if (mainCam == null) yield break;
        
        float startSize = mainCam.orthographicSize;
        float elapsedTime = 0f;
        
        while (elapsedTime < zoomDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = elapsedTime / zoomDuration;
            
            float currentSize = Mathf.Lerp(startSize, targetSize, progress);
            mainCam.orthographicSize = currentSize;
            
            // Update camera boundaries if exists
            CameraBoundaries boundaries = FindFirstObjectByType<CameraBoundaries>();
            if (boundaries != null)
            {
                boundaries.UpdateCameraSize();
            }
            
            yield return null;
        }
        
        mainCam.orthographicSize = targetSize;
    }
    
    public void DisableAllBosses()
    {
        foreach (var bossData in bossDataList)
        {
            if (bossData.miniBoss != null)
            {
                bossData.miniBoss.enabled = false;
            }
            
            if (bossData.damageReceiver != null)
            {
                bossData.damageReceiver.enabled = false;
            }
            
            if (bossData.rb != null)
            {
                bossData.rb.bodyType = RigidbodyType2D.Static;
                bossData.rb.linearVelocity = Vector2.zero;
            }
            
            // Disable colliders
            foreach (var collider in bossData.collidersToControl)
            {
                if (collider != null)
                {
                    collider.enabled = false;
                }
            }
        }
        
        Debug.Log($"[BossWarningManager] Disabled {bossDataList.Count} boss(es)");
    }
    
    public void EnableAllBosses()
    {
        foreach (var bossData in bossDataList)
        {
            if (bossData.miniBoss != null)
            {
                bossData.miniBoss.enabled = true;
            }
            
            if (bossData.damageReceiver != null)
            {
                bossData.damageReceiver.enabled = true;
            }
            
            if (bossData.rb != null)
            {
                bossData.rb.bodyType = RigidbodyType2D.Dynamic;
            }
            
            // Enable colliders
            foreach (var collider in bossData.collidersToControl)
            {
                if (collider != null)
                {
                    collider.enabled = true;
                }
            }
        }
        
        Debug.Log($"[BossWarningManager] Enabled {bossDataList.Count} boss(es)");
    }
    
    // Public methods for external control
    public void AddBossData(BossWarningData bossData)
    {
        if (!bossDataList.Contains(bossData))
        {
            bossDataList.Add(bossData);
        }
    }
    
    public void RemoveBossData(BossWarningData bossData)
    {
        if (bossDataList.Contains(bossData))
        {
            bossDataList.Remove(bossData);
        }
    }
    
    public void SetWarningMessage(string message)
    {
        warningMessage = message;
        originalWarningText = message;
    }
    
    // Method to check if any boss has been triggered before
    public bool HasAnyBossBeenTriggered()
    {
        foreach (var bossData in bossDataList)
        {
            if (bossData.hasBeenTriggeredBefore)
                return true;
        }
        return false;
    }
}