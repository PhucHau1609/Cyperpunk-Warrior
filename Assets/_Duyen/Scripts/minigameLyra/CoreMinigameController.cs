using UnityEngine;
using UnityEngine.UI;

public class CoreMinigameController : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform targetZone;
    public RectTransform lyra;
    public RectTransform movementBounds;
    public Image fillBar;

    [Header("Lyra Movement")]
    public float lyraSpeed = 200f;

    [Header("Progress Settings")]
    public float chargeTime = 5f;
    public float decayRate = 1f;

    [Header("Target Movement Randomization")]
    public float minSpeed = 50f;
    public float maxSpeed = 200f;
    public float randomizeInterval = 1.5f;

    public System.Action onComplete;

    private float progress = 0f;
    private bool isPlaying = false;
    private Vector2 targetDirection = Vector2.right;
    [HideInInspector] public float targetSpeed;
    private float randomizeTimer = 0f;

    public GameObject lyraObject; // ← Lyra thật bên ngoài
    private PetManualControl manualControl;
    private PetShooting petShooting;

    public void StartMinigame()
    {
        progress = 0f;
        fillBar.fillAmount = 0f;
        isPlaying = true;
        gameObject.SetActive(true);

        if (lyraObject != null)
        {
            manualControl = lyraObject.GetComponent<PetManualControl>();
            petShooting = lyraObject.GetComponent<PetShooting>();

            if (manualControl != null) manualControl.enabled = false;
            if (petShooting != null) petShooting.enabled = false;
        }

        RandomizeTargetMovement();
        randomizeTimer = 0f;
    }

    public void StopMinigame()
    {
        isPlaying = false;
        gameObject.SetActive(false);

        if (manualControl != null) manualControl.enabled = true;
        if (petShooting != null) petShooting.enabled = true;
    }

    void Update()
    {
        if (!isPlaying) return;

        randomizeTimer += Time.deltaTime;
        if (randomizeTimer >= randomizeInterval)
        {
            randomizeTimer = 0f;
            RandomizeTargetMovement();
        }

        MoveTarget();
        MoveLyra();
        UpdateProgress();
    }

    void RandomizeTargetMovement()
    {
        targetDirection = Random.value > 0.5f ? Vector2.right : Vector2.left;
        targetSpeed = Random.Range(minSpeed, maxSpeed);
    }

    void MoveTarget()
    {
        Vector2 pos = targetZone.anchoredPosition;
        pos += targetDirection * targetSpeed * Time.deltaTime;

        float halfWidth = targetZone.rect.width / 2f;
        float minX = movementBounds.rect.xMin + halfWidth;
        float maxX = movementBounds.rect.xMax - halfWidth;

        // Clamp nếu vượt biên và đổi hướng
        if (pos.x < minX)
        {
            pos.x = minX;
            targetDirection = Vector2.right;
        }
        else if (pos.x > maxX)
        {
            pos.x = maxX;
            targetDirection = Vector2.left;
        }

        targetZone.anchoredPosition = pos;
    }

    void MoveLyra()
    {
        float move = Input.GetAxis("Horizontal");

        // Di chuyển vị trí
        Vector2 pos = lyra.anchoredPosition;
        pos.x += move * lyraSpeed * Time.deltaTime;
        pos.x = Mathf.Clamp(pos.x, movementBounds.rect.xMin, movementBounds.rect.xMax);
        lyra.anchoredPosition = pos;

        // Xoay hướng (flip)
        if (Mathf.Abs(move) > 0.01f)
        {
            Vector3 scale = lyra.localScale;
            scale.x = move > 0 ? 1f : -1f;
            lyra.localScale = scale;
        }

        // Luôn chạy animation duy nhất nếu có Animator
        Animator animator = lyra.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetBool("isMoving", Mathf.Abs(move) > 0.01f);
        }
        //float move = Input.GetAxis("Horizontal");
        //Vector2 pos = lyra.anchoredPosition;
        //pos.x += move * lyraSpeed * Time.deltaTime;
        //pos.x = Mathf.Clamp(pos.x, movementBounds.rect.xMin, movementBounds.rect.xMax);
        //lyra.anchoredPosition = pos;
    }

    void UpdateProgress()
    {
        if (RectOverlaps(lyra, targetZone))
        {
            progress += Time.deltaTime;
        }
        else
        {
            progress -= Time.deltaTime * decayRate;
        }

        progress = Mathf.Clamp(progress, 0f, chargeTime);
        fillBar.fillAmount = progress / chargeTime;

        if (progress >= chargeTime)
        {
            StopMinigame();
            onComplete?.Invoke();
        }
    }

    bool RectOverlaps(RectTransform a, RectTransform b)
    {
        Rect ra = GetWorldRect(a);
        Rect rb = GetWorldRect(b);
        return ra.Overlaps(rb);
    }

    Rect GetWorldRect(RectTransform rt)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);

        Vector2 bottomLeft = corners[0];
        Vector2 topRight = corners[2];

        // Sửa nếu width âm (khi scale.x < 0)
        if (topRight.x < bottomLeft.x)
        {
            float temp = topRight.x;
            topRight.x = bottomLeft.x;
            bottomLeft.x = temp;
        }

        return new Rect(bottomLeft, topRight - bottomLeft);
    }
    //Rect GetWorldRect(RectTransform rt)
    //{
    //    Vector3[] corners = new Vector3[4];
    //    rt.GetWorldCorners(corners);

    //    float minX = Mathf.Min(corners[0].x, corners[2].x);
    //    float maxX = Mathf.Max(corners[0].x, corners[2].x);
    //    float minY = Mathf.Min(corners[0].y, corners[2].y);
    //    float maxY = Mathf.Max(corners[0].y, corners[2].y);

    //    return new Rect(new Vector2(minX, minY), new Vector2(maxX - minX, maxY - minY));
    //}

    //Rect GetWorldRect(RectTransform rt)
    //{
    //    Vector3[] corners = new Vector3[4];
    //    rt.GetWorldCorners(corners);
    //    return new Rect(corners[0], corners[2] - corners[0]);
    //}
}
