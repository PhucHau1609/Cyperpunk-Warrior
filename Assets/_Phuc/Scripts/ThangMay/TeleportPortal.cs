using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;


public class TeleportPortal : MonoBehaviour
{
    public int targetSceneIndex = 2; // 👉 Index scene trong Build Settings
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

        // 👉 THÊM DÒNG NÀY: Thiết lập SpawnPoint cho scene kế tiếp
        if (SpawnManager.Instance != null)
            SpawnManager.Instance.SetNextSpawnPoint(SpawnSceneName.MapLevel2);

        // 👉 Lưu index scene cần load vào PlayerPrefs
        PlayerPrefs.SetInt("NextSceneIndex", targetSceneIndex);

        // 👉 Load scene loading
        SceneManager.LoadScene(loadingSceneName);
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
