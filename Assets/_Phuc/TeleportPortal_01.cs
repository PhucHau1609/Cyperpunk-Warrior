using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TeleportPortal_01 : MonoBehaviour
{
    public int targetSceneIndex = 2; // 👉 Index scene trong Build Settings
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

        // 👉 Nếu bạn dùng hệ thống spawn, hãy set lại điểm spawn tại đây
        if (SpawnManager.Instance != null)
            SpawnManager.Instance.SetNextSpawnPoint(SpawnSceneName.MapLevel2); // Tùy chỉnh nếu cần

        PlayerPrefs.SetInt("NextSceneIndex", targetSceneIndex);

        SceneManager.LoadScene(loadingSceneName);
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
