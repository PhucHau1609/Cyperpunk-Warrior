using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;


public class ItemPickupFlyUI : HauSingleton<ItemPickupFlyUI>
{
    [Header("Refs")]
    [SerializeField] private Canvas canvas;                       // Screen Space - Overlay
    [SerializeField] private RectTransform targetInventoryIcon;   // Đích UI
    [SerializeField] private Image iconPrefab;
    [SerializeField] private Camera worldCamera;                  // Camera render world (gán Camera.main hoặc camera gameplay)

    [Header("Effect Config")]
    [SerializeField] private float duration = 0.55f;
    [SerializeField] private bool useUnscaledTime = true;
    [SerializeField] private float curveHeight = 120f;
    [SerializeField] private Ease moveEase = Ease.InOutQuad;

    [Header("Size/Scale")]
    [SerializeField] private bool useFixedSize = true;
    [SerializeField] private Vector2 fixedSize = new Vector2(64, 64);
    [SerializeField] private float startScale = 0.9f;
    [SerializeField] private float endScale = 0.55f;
    [Range(0f, 1f)][SerializeField] private float endAlpha = 0.85f;

    protected override void Awake()
    {
        base.Awake();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Gán lại camera mỗi khi load scene mới
        if (worldCamera == null) worldCamera = Camera.main; // đảm bảo có camera
    }

    protected override void LoadComponents()
    {
        base.LoadComponents();
        if (canvas == null) canvas = GetComponentInParent<Canvas>();
        if (worldCamera == null) worldCamera = Camera.main; // đảm bảo có camera
    }

    public void SetTarget(RectTransform target) => targetInventoryIcon = target;
    public void SetWorldCamera(Camera cam) => worldCamera = cam;

    // --- API chính: truyền ItemCode + vị trí world của item (hoặc transform item để tự bắt bounds.center) ---
    public void Play(ItemCode itemCode, Vector3 worldStart, Sprite fallbackSprite = null,
                     float? overrideDuration = null, Vector2? overrideFixedSize = null)
    {
        if (!IsReady()) return;

        // Lấy sprite từ profile (đổi lại field cho đúng SO của bạn)
        Sprite sprite = fallbackSprite;
        var profile = InventoryManager.Instance.GetProfileByCode(itemCode);
        if (profile != null && profile.itemSprite != null) sprite = profile.itemSprite;
        if (sprite == null) return;

        SpawnAndAnimate(sprite, worldStart, overrideDuration ?? duration, overrideFixedSize ?? fixedSize);
    }

    // Overload: truyền trực tiếp Transform của item -> lấy bounds.center để chính xác thị giác
    public void PlayFromTransform(ItemCode itemCode, Transform itemTf, Sprite fallbackSprite = null,
                                  float? overrideDuration = null, Vector2? overrideFixedSize = null)
    {
        if (!IsReady() || itemTf == null) return;

        // Tính tâm hiển thị (ưu tiên Renderer -> Collider -> position)
        Vector3 startWorld = GetVisualCenter(itemTf);

        Sprite sprite = fallbackSprite;
        var profile = InventoryManager.Instance.GetProfileByCode(itemCode);
        if (profile != null && profile.itemSprite != null) sprite = profile.itemSprite;
        if (sprite == null) return;

        SpawnAndAnimate(sprite, startWorld, overrideDuration ?? duration, overrideFixedSize ?? fixedSize);
    }

    // -------------------- Internal --------------------
    private bool IsReady()
    {
        return canvas != null && targetInventoryIcon != null && iconPrefab != null && worldCamera != null;
    }

    private void SpawnAndAnimate(Sprite sprite, Vector3 worldStart, float d, Vector2 sizePx)
    {
        // Tạo image tạm
        Image img = Instantiate(iconPrefab, canvas.transform);
        img.sprite = sprite;
        img.color = Color.white;

        var rt = img.rectTransform;
        var cg = img.GetComponent<CanvasGroup>() ?? img.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 1f;

        if (useFixedSize) rt.sizeDelta = sizePx; else img.SetNativeSize();

        RectTransform canvasRT = (RectTransform)canvas.transform;
        rt.SetParent(canvasRT, false);

        // ---- CHUYỂN TỌA ĐỘ CHUẨN CHO OVERLAY ----
        // 1) world -> screen bằng camera đang render world
        Vector2 startScreen = worldCamera.WorldToScreenPoint(worldStart);
        // 2) screen -> local(anchored) trong canvas overlay (camera tham chiếu = null)
        Vector2 startAnch = ScreenToCanvasLocal(startScreen, canvasRT);

        // Tính end bằng pipeline giống hệt:
        Vector2 endScreen = RectTransformUtility.WorldToScreenPoint(null, targetInventoryIcon.position);
        Vector2 endAnch = ScreenToCanvasLocal(endScreen, canvasRT);

        rt.anchoredPosition = startAnch;
        rt.localScale = Vector3.one * startScale;

        // Control point cho đường cong
        Vector2 control = (startAnch + endAnch) * 0.5f + new Vector2(0f, curveHeight);

        // DOTween sequence
        var seq = DOTween.Sequence().SetUpdate(useUnscaledTime);

        float t = 0f;
        Tween pathTween = DOTween
            .To(() => t, x => {
                t = x;
                rt.anchoredPosition = QuadraticBezier(startAnch, control, endAnch, t);
            }, 1f, d)
            .SetEase(moveEase);

        seq.Join(pathTween)
           .Join(rt.DOScale(endScale, d))
           .Join(cg.DOFade(endAlpha, d))
           .OnComplete(() => { if (img) Destroy(img.gameObject); });
    }

    private static Vector2 ScreenToCanvasLocal(Vector2 screenPos, RectTransform canvasRT)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRT, screenPos, null, out Vector2 local);
        return local;
    }

    private static Vector3 GetVisualCenter(Transform t)
    {
        if (t.TryGetComponent<Renderer>(out var r)) return r.bounds.center;
        if (t.TryGetComponent<Collider>(out var c)) return c.bounds.center;
        return t.position;
    }

    private static Vector2 QuadraticBezier(Vector2 a, Vector2 b, Vector2 c, float t)
    {
        Vector2 ab = Vector2.Lerp(a, b, t);
        Vector2 bc = Vector2.Lerp(b, c, t);
        return Vector2.Lerp(ab, bc, t);
    }
}
