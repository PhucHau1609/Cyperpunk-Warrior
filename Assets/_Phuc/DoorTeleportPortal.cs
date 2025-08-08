using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorTeleportPortal : MonoBehaviour
{
    public string loadingSceneName = "LoadingScene";
    public AudioClip teleportSound;
    public SpawnSceneName nextSpawnPoint;

    private AudioSource audioSource;
    private Animator portalAnimator;
    private Animator doorAnimator;
    private bool isTeleporting = false;
    public bool isUnlocked = true;
    private bool hasTriggeredDisappear = false;
    private bool hasClosedDoor = false;

    [Header("Door Animator Settings")]
    public GameObject doorObject; // Assign the door GameObject with Animator in Inspector

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        portalAnimator = GetComponent<Animator>();
        if (portalAnimator == null)
            portalAnimator = GetComponentInChildren<Animator>();

        if (doorObject != null)
            doorAnimator = doorObject.GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isUnlocked || isTeleporting || !other.CompareTag("Player"))
            return;

        isTeleporting = true;

        WeaponSystemManager weapon = other.GetComponent<WeaponSystemManager>();
        if (weapon != null)
        {
            weapon.isWeaponActive = false;
            var weaponHolderField = other.transform.Find("WeaponHandle");
            if (weaponHolderField != null)
            {
                foreach (Transform child in weaponHolderField)
                {
                    child.gameObject.SetActive(false);
                }
            }
        }

        Animator playerAnim = other.GetComponentInChildren<Animator>();
        if (playerAnim != null && !hasTriggeredDisappear)
        {
            playerAnim.ResetTrigger("PlayDisappear");
            playerAnim.SetTrigger("PlayDisappear");
            hasTriggeredDisappear = true;
        }

        FloatingFollower pet = FindFirstObjectByType<FloatingFollower>();
        if (pet != null)
        {
            pet.Disappear();
        }

        if (teleportSound != null)
            audioSource.PlayOneShot(teleportSound);

        // Trigger close-door animation
        if (doorAnimator != null && !hasClosedDoor)
        {
            doorAnimator.ResetTrigger("CloseDoor");
            doorAnimator.SetTrigger("CloseDoor");
            hasClosedDoor = true;
        }

        StartCoroutine(WaitForDisappearAnimation(playerAnim));
    }

    private IEnumerator WaitForDisappearAnimation(Animator playerAnim)
    {
        float animLength = (playerAnim != null)
            ? playerAnim.GetCurrentAnimatorStateInfo(0).length
            : 1f;

        yield return new WaitForSeconds(animLength);

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
            Debug.LogWarning("No next scene in Build Settings.");
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