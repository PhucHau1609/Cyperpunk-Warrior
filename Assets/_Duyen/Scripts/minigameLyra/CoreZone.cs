using UnityEngine;
using System.Collections;

public class CoreZone : MonoBehaviour
{
    [Header("Charging Settings")]
    public float chargeTime = 5f;
    public float decayRate = 1f;
    public float delayBeforeStart = 1f;

    [Header("UI")]
    public UIProgressBar uiBar;

    [Header("Visuals")]
    public Sprite completedSprite;

    private float progress = 0f;
    private float stayTime = 0f;
    private bool isCharging = false;
    private bool lyraInside = false;
    private bool canBeInteracted = false;

    private CoreManager coreManager;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        coreManager = FindAnyObjectByType<CoreManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        uiBar?.SetProgress(0f);
        uiBar?.Show(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!canBeInteracted) return;

        if (other.CompareTag("NPC"))
        {
            lyraInside = true;
            stayTime = 0f;
            uiBar?.Show(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!canBeInteracted) return;

        if (other.CompareTag("NPC"))
        {
            lyraInside = false;
            stayTime = 0f;
            isCharging = false;
            StopAllCoroutines();
            StartCoroutine(DecayProgress());
        }
    }

    void Update()
    {
        if (!canBeInteracted) return;

        if (lyraInside && !isCharging)
        {
            stayTime += Time.deltaTime;
            if (stayTime >= delayBeforeStart)
            {
                StopAllCoroutines();
                StartCoroutine(ChargeCore());
            }
        }
    }

    IEnumerator ChargeCore()
    {
        isCharging = true;
        while (lyraInside && progress < chargeTime)
        {
            progress += Time.deltaTime;
            uiBar.SetProgress(progress / chargeTime);
            yield return null;
        }

        if (progress >= chargeTime)
        {
            progress = chargeTime;
            uiBar.SetProgress(1f);
            uiBar.Show(false);
            canBeInteracted = false;

            coreManager.MarkCoreAsComplete(this);
            SetAsCompletedVisual();
        }

        isCharging = false;
    }

    IEnumerator DecayProgress()
    {
        while (!lyraInside && progress > 0f)
        {
            progress -= Time.deltaTime * decayRate;
            uiBar.SetProgress(progress / chargeTime);
            yield return null;
        }

        if (progress <= 0f)
        {
            progress = 0f;
            uiBar?.Show(false);
        }
    }

    public void SetActiveLogic(bool active)
    {
        canBeInteracted = active;
        if (!active)
        {
            lyraInside = false;
            StopAllCoroutines();
            uiBar?.Show(false);
        }
    }

    public void SetAsCompletedVisual()
    {
        if (spriteRenderer != null && completedSprite != null)
        {
            spriteRenderer.sprite = completedSprite;
        }
    }
}
