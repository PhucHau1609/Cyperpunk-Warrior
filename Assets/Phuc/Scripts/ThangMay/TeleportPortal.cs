using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;

public class TeleportPortal : MonoBehaviour
{
    public string targetSceneName = "MapLevel4";
    public AudioClip teleportSound;
    private AudioSource audioSource;

    private bool isTeleporting = false;

    [System.NonSerialized]
    private bool isUnlocked = false; 

    private GameObject player;

    private void Start()
    {
        isUnlocked = false;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isUnlocked) return; 

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

        SceneManager.LoadScene(targetSceneName);
    }


    public void UnlockPortal()
    {
        isUnlocked = true;
        Debug.Log("Cổng dịch chuyển đã được mở!");
    }


    public void ResetPortal()
    {
        isUnlocked = false;
        Debug.Log("Cổng dịch chuyển đã bị khóa lại.");
    }
}
