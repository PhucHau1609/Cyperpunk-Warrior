using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CoreZone : MonoBehaviour
{
    [Header("Reward")]
    [SerializeField] protected ItemCode rewardCore;
    [SerializeField] protected Vector3 spawnCore;

    [Header("Minigame")]
    public GameObject minigamePrefab; 
    public float targetSpeed = 100f; 

    private bool canBeInteracted = false;
    private CoreManager coreManager;
    private GameObject spawnedMinigame;

    private bool minigameRunning = false;
    private LyraHealth lyraHealth;

    private bool initialCanBeInteracted = false;
    private bool initialColliderState = true;

    private bool isResetting = false;
    private bool hasNpcInside = false;

    private bool isCompleted = false;

    void Start()
    {
        coreManager = FindAnyObjectByType<CoreManager>();

        initialCanBeInteracted = canBeInteracted;

        Collider2D[] colliders = GetComponents<Collider2D>();
        if (colliders.Length > 0)
        {
            foreach (var col in colliders)
            {
                if (!col.isTrigger)
                {
                    initialColliderState = col.enabled;
                    break;
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("NPC"))
        {
            hasNpcInside = true;
        }

        if (!canBeInteracted || isResetting || minigameRunning || isCompleted)
        {
            return;
        }

        if (other.CompareTag("NPC"))
        {
            if (minigamePrefab != null && spawnedMinigame == null)
            {
                minigameRunning = true;
                canBeInteracted = false;

                GameObject lyraObj = GameObject.FindGameObjectWithTag("NPC");
                if (lyraObj != null)
                {
                    lyraHealth = lyraObj.GetComponent<LyraHealth>();
                    if (lyraHealth != null)
                    {
                        lyraHealth.OnDeath += HandleNpcDeath;
                    }
                }

                Transform canvas = GameObject.Find("bar_ca").transform;
                spawnedMinigame = Instantiate(minigamePrefab, canvas);

                var minigame = spawnedMinigame.GetComponent<CoreMinigameController>();
                minigame.targetSpeed = targetSpeed;
                minigame.lyraObject = lyraObj;

                minigame.onComplete = () =>
                {
                    minigameRunning = false;
                    isCompleted = true;
                    
                    coreManager.MarkCoreAsComplete(this);
                    ItemsDropManager.Instance.DropItem(rewardCore, 1, this.transform.position + spawnCore);
                    Destroy(spawnedMinigame);
                    spawnedMinigame = null;
                    StartCoroutine(OpenDoorSequence());
                };

                minigame.StartMinigame();
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("NPC"))
        {
            hasNpcInside = false;
        }
    }

    private void HandleNpcDeath()
    {
        if (minigameRunning)
        {
            minigameRunning = false;
            Destroy(spawnedMinigame);
            if (spawnedMinigame != null)
            {
                var minigame = spawnedMinigame.GetComponent<CoreMinigameController>();
                if (minigame != null)
                {
                    minigame.StopMinigame();
                }
            }
            spawnedMinigame = null;

            if (hasNpcInside && coreManager != null && !isCompleted)
            {
                int coreIndex = coreManager.cores.IndexOf(this);
                if (coreIndex >= 0 && coreIndex == coreManager.GetCurrentCoreIndex())
                {
                    canBeInteracted = true;
                }
            }

            if (lyraHealth != null)
                lyraHealth.OnDeath -= HandleNpcDeath;
        }
    }

    public void SetActiveLogic(bool active)
    {
        
        if (isResetting)
        {
            return;
        }

        if (isCompleted)
        {
            canBeInteracted = false;
            return;
        }
        
        canBeInteracted = active;

        if (active && hasNpcInside)
        {
            StartCoroutine(DelayedActivation());
        }
    }

    private IEnumerator DelayedActivation()
    {
        canBeInteracted = false;
        yield return new WaitForSeconds(0.5f);
        
        if (!isResetting && !minigameRunning && !isCompleted)
        {
            canBeInteracted = true;
        }
    }

    public void SetAsCompleted()
    {
        isCompleted = true;
        canBeInteracted = false;
        minigameRunning = false;

        StartCoroutine(OpenDoorSequence());
    }

    public void ResetCoreForMiniGame()
    {
        isResetting = true;
        canBeInteracted = false;

        if (spawnedMinigame != null)
        {
            Destroy(spawnedMinigame);
            spawnedMinigame = null;
        }

        minigameRunning = false;

        if (lyraHealth != null)
        {
            lyraHealth.OnDeath -= HandleNpcDeath;
            lyraHealth = null;
        }

        if (!isCompleted)
        {
            Collider2D[] colliders = GetComponents<Collider2D>();
            foreach (var col in colliders)
            {
                if (!col.isTrigger)
                {
                    col.enabled = initialColliderState;
                }
            }

            Animator doorAnimator = GetComponent<Animator>();
            if (doorAnimator != null)
            {
                doorAnimator.Play("ClosedState", 0, 0f);
            }
        }

        StartCoroutine(FinishReset());
    }

    public void ResetCompletion()
    {
        isCompleted = false;
    }

    private IEnumerator FinishReset()
    {
        yield return new WaitForSeconds(1f);
        
        isResetting = false;
    }

    private IEnumerator OpenDoorSequence()
    {
        yield return new WaitForSeconds(0.2f);

        Animator doorAnimator = GetComponent<Animator>();
        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger("Open");
        }

        yield return new WaitForSeconds(0.5f);
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (var col in colliders)
        {
            if (!col.isTrigger)
            {
                col.enabled = false;
            }
        }
    }

    public bool IsCompleted()
    {
        return isCompleted;
    }
}