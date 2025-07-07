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
    public Sprite sprite30;
    public Sprite sprite70;
    public Sprite sprite100;

    private float progress = 0f;
    private float stayTime = 0f;
    private bool isCharging = false;
    private bool lyraInside = false;
    private bool canBeInteracted = false;

    private CoreManager coreManager;
    private SpriteRenderer spriteRenderer;

    // Mốc hiện tại để tránh đổi sprite nhiều lần
    private int currentStage = 0;

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
            float percent = progress / chargeTime;
            uiBar.SetProgress(percent);

            UpdateSpriteByProgress(percent * 100f);

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
            float percent = Mathf.Clamp01(progress / chargeTime);
            uiBar.SetProgress(percent);

            UpdateSpriteByProgress(percent * 100f);

            yield return null;
        }

        if (progress <= 0f)
        {
            progress = 0f;
            currentStage = 0;
            uiBar?.Show(false);
        }
    }

    void UpdateSpriteByProgress(float percent)
    {
        if (spriteRenderer == null) return;

        if (percent >= 100 && currentStage < 3)
        {
            spriteRenderer.sprite = sprite100;
            currentStage = 3;
        }
        else if (percent >= 70 && currentStage < 2)
        {
            spriteRenderer.sprite = sprite70;
            currentStage = 2;
        }
        else if (percent >= 30 && currentStage < 1)
        {
            spriteRenderer.sprite = sprite30;
            currentStage = 1;
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
        UpdateSpriteByProgress(100f); // đảm bảo hiển thị sprite 100%
    }
}
