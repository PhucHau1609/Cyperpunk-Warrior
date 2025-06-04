using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TeleportPortal_01 : MonoBehaviour
{
    public string loadingSceneName = "LoadingScene"; // Tên scene loading
    public AudioClip teleportSound;
    public SpawnSceneName nextSpawnPoint;

    private AudioSource audioSource;
    private Animator portalAnimator;
    private bool isTeleporting = false;
    public bool isUnlocked = true;
    private bool hasTriggeredDisappear = false; // ✅ Đảm bảo animation chỉ gọi 1 lần

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        portalAnimator = GetComponent<Animator>();
        if (portalAnimator == null)
            portalAnimator = GetComponentInChildren<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isUnlocked || isTeleporting || !other.CompareTag("Player"))
            return;

        isTeleporting = true;

        // ✅ Gọi animation biến mất của player nếu chưa gọi
        Animator playerAnim = other.GetComponentInChildren<Animator>();
        if (playerAnim != null && !hasTriggeredDisappear)
        {
            playerAnim.ResetTrigger("PlayDisappear");
            playerAnim.SetTrigger("PlayDisappear");
            hasTriggeredDisappear = true;
        }

        // ✅ Gọi animation biến mất của pet
        FloatingFollower pet = FindFirstObjectByType<FloatingFollower>();
        if (pet != null)
        {
            pet.Disappear();
        }

        if (teleportSound != null)
            audioSource.PlayOneShot(teleportSound);

        StartCoroutine(WaitForDisappearAnimation(playerAnim));
    }

    private IEnumerator WaitForDisappearAnimation(Animator playerAnim)
    {
        float animLength = (playerAnim != null)
            ? playerAnim.GetCurrentAnimatorStateInfo(0).length
            : 1f;

        yield return new WaitForSeconds(animLength);

        // 👉 Đặt điểm spawn cho scene tiếp theo
        if (SpawnManager.Instance != null)
            SpawnManager.Instance.SetNextSpawnPoint(nextSpawnPoint);

        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            PlayerPrefs.SetInt("NextSceneIndex", nextSceneIndex);
            SceneManager.LoadScene(loadingSceneName);
        }
        else
        {
            Debug.LogWarning("Không còn scene tiếp theo trong Build Settings.");
        }
    }

    public void UnlockPortal()
    {
        isUnlocked = true;
        SetGateAnimation(true);
    }

    public void LockPortal()
    {
        isUnlocked = false;
        SetGateAnimation(false);
    }

    private void SetGateAnimation(bool isWorking)
    {
        if (portalAnimator != null)
        {
            portalAnimator.SetBool("IsGateWorking", isWorking);
        }
    }
}
