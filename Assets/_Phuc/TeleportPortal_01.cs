using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TeleportPortal_01 : MonoBehaviour
{
    public string loadingSceneName = "LoadingScene"; // Tên scene loading
    public AudioClip teleportSound;

    private AudioSource audioSource;
    private Animator portalAnimator;
    private bool isTeleporting = false;
    public bool isUnlocked = true; // 👉 Đặt mặc định là true để cổng hoạt động ngay

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

        Animator playerAnim = other.GetComponent<Animator>();
        if (playerAnim != null)
            playerAnim.SetTrigger("PlayDisappear");

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

        // 👉 Thiết lập điểm spawn nếu cần
        if (SpawnManager.Instance != null)
            SpawnManager.Instance.SetNextSpawnPoint(SpawnSceneName.MapLevel2); // Tùy chỉnh nếu có

        // 👉 Tự động lấy scene hiện tại + 1
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        // 👉 Kiểm tra xem scene tiếp theo có tồn tại không
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            PlayerPrefs.SetInt("NextSceneIndex", nextSceneIndex);
            SceneManager.LoadScene(loadingSceneName);
        }
        else
        {
            Debug.LogWarning("Không còn scene tiếp theo trong Build Settings.");
            // 👉 Bạn có thể chuyển về menu hoặc restart nếu muốn
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
