using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;

public class TeleportPortal : MonoBehaviour
{
    public string targetSceneName = "MapLevel2"; // Tên scene cần chuyển
    public AudioClip teleportSound;              // Âm thanh dịch chuyển
    private AudioSource audioSource;             // AudioSource để phát âm thanh

    private bool isTeleporting = false;
    private GameObject player;

    private void Start()
    {
        // Gắn sẵn AudioSource hoặc thêm nếu chưa có
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

            // Gọi animation Disappear của Player
            Animator playerAnim = player.GetComponent<Animator>();
            if (playerAnim != null)
            {
                playerAnim.SetTrigger("PlayDisappear");
            }

            // Phát âm thanh dịch chuyển
            if (teleportSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(teleportSound);
            }

            // Bắt đầu Coroutine để đợi animation hoàn tất
            StartCoroutine(WaitForAnimationToFinish(playerAnim));
        }
    }

    private IEnumerator WaitForAnimationToFinish(Animator playerAnim)
    {
        float animationLength = playerAnim.GetCurrentAnimatorStateInfo(0).length;

        // Nếu không lấy được length đúng, đặt thời gian mặc định
        if (animationLength <= 0f)
            animationLength = 1f;

        // Đợi animation + thời gian âm thanh nếu cần
        yield return new WaitForSeconds(animationLength);

        // Chuyển scene
        SceneManager.LoadScene(targetSceneName);
    }
}
