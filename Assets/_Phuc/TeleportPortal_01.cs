using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TeleportPortal_01 : MonoBehaviour
{
    public AudioClip teleportSound;
    private AudioSource audioSource;

    private bool isTeleporting = false;
    private GameObject player;

    [SerializeField] private int nextSceneIndex = 1; // 👈 Thứ tự Scene trong Build Settings
    [SerializeField] private string loadingSceneName = "LoadingScene"; // 👈 Scene loading bạn đã tạo

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isTeleporting && other.CompareTag("Player"))
        {
            isTeleporting = true;
            player = other.gameObject;

            Animator playerAnim = player.GetComponent<Animator>();
            if (playerAnim != null)
            {
                playerAnim.SetTrigger("PlayDisappear");
            }

            if (teleportSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(teleportSound);
            }

            StartCoroutine(WaitForAnimationToFinish(playerAnim));
        }
    }

    private IEnumerator WaitForAnimationToFinish(Animator playerAnim)
    {
        float animationLength = playerAnim != null ? playerAnim.GetCurrentAnimatorStateInfo(0).length : 1f;

        if (animationLength <= 0f)
            animationLength = 1f;

        yield return new WaitForSeconds(animationLength);

        if (SpawnManager.Instance != null)
            SpawnManager.Instance.SetNextSpawnPoint(SpawnSceneName.MapLevel3);

        // Lưu chỉ số scene kế tiếp để màn hình loading sử dụng
        PlayerPrefs.SetInt("NextSceneIndex", nextSceneIndex);
        PlayerPrefs.Save();

        // Load màn hình loading
        SceneManager.LoadScene(loadingSceneName);
    }
}
