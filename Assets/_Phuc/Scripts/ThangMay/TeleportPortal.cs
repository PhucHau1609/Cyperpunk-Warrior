using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class TeleportPortal : MonoBehaviour
{
    public string loadingSceneName = "LoadingScene"; // Tên scene loading
    public AudioClip teleportSound;

    private AudioSource audioSource;
    private Animator portalAnimator;
    private bool isTeleporting = false;
    private bool isUnlocked = false;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        portalAnimator = GetComponent<Animator>();
        if (portalAnimator == null)
            portalAnimator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        ResetPortal();
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

        // 👉 Thiết lập điểm spawn nếu có
        if (SpawnManager.Instance != null)
            SpawnManager.Instance.SetNextSpawnPoint(SpawnSceneName.map1level2);

        // 👉 Tự động tăng index scene
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        // 👉 Kiểm tra nếu còn scene trong Build Settings
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            PlayerPrefs.SetInt("NextSceneIndex", nextSceneIndex);
            SceneManager.LoadScene(loadingSceneName);
        }
        else
        {
            Debug.Log("⚠ Không còn scene tiếp theo trong Build Settings!");
            // Có thể chuyển về menu chính hoặc restart game tại đây nếu bạn muốn
        }
    }

    public void UnlockPortal()
    {
        isUnlocked = true;
        SetGateAnimation(true);
    }

    public void ResetPortal()
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
