using UnityEngine;
using DG.Tweening;

public class SwapTargetManager : MonoBehaviour
{
    public static SwapTargetManager Instance;

    public Transform player;
    private SwapableObject currentTarget;
    private bool isSwapping = false;

    [Header("Swap Settings")]
    public float swapDuration = 0.3f;
    public Ease swapEase = Ease.InOutSine;

    [Tooltip("Tăng tốc / làm chậm toàn bộ hiệu ứng swap")]
    public float swapSpeedMultiplier = 1.0f;

    [Tooltip("Làm chậm thời gian khi swap (0.1 = slow motion)")]
    public float slowMotionScale = 0.1f;

    private Animator playerAnimator;

    void Awake()
    {
        Instance = this;
        if (player != null) playerAnimator = player.GetComponentInChildren<Animator>();
    }

    public void SetTarget(SwapableObject target, bool selected)
    {
        if (selected)
        {
            if (currentTarget != null && currentTarget != target)
            {
                currentTarget.isSelected = false;
                currentTarget.GetComponent<SpriteRenderer>().color = Color.white;
            }
            currentTarget = target;
        }
        else
        {
            if (currentTarget == target)
            {
                currentTarget = null;
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && currentTarget != null && !isSwapping)
        {
            SwapWithEffect();
        }
    }

    void SwapWithEffect()
    {
        if (currentTarget == null || player == null) return;

        isSwapping = true;

        Vector3 playerStartPos = player.position;
        Vector3 targetStartPos = currentTarget.transform.position;

        Vector3 playerStartScale = player.localScale;
        Vector3 targetStartScale = currentTarget.transform.localScale;
        //Debug.Log($"Before swap: playerScale.x = {playerStartScale.x}, targetScale.x = {targetStartScale.x}");


        float baseDuration = swapDuration / Mathf.Max(swapSpeedMultiplier, 0.01f);

        float scaleDuration = baseDuration * 0.3f;
        float moveDuration = baseDuration * 0.4f;
        float scaleBackDuration = baseDuration * 0.3f;

        Time.timeScale = slowMotionScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        if (playerAnimator != null) playerAnimator.speed = 0f;

        // Thu nhỏ theo đúng hướng scale hiện tại
        Vector3 playerShrinkScale = playerStartScale * 0.4f;
        Vector3 targetShrinkScale = targetStartScale * 0.4f;


        Sequence s = DOTween.Sequence();

        // Giai đoạn 1: thu nhỏ
        s.Append(player.DOScale(playerShrinkScale, scaleDuration).SetEase(Ease.InOutQuad));
        s.Join(currentTarget.transform.DOScale(targetShrinkScale, scaleDuration).SetEase(Ease.InOutQuad));

        // Giai đoạn 2: hoán đổi vị trí
        s.Append(player.DOMove(targetStartPos, moveDuration).SetEase(swapEase));
        s.Join(currentTarget.transform.DOMove(playerStartPos, moveDuration).SetEase(swapEase));

        // Giai đoạn 3: phóng to lại, nhưng dùng scale của object đích
        s.Append(player.DOScale(playerStartScale, scaleBackDuration).SetEase(Ease.InOutQuad));
        s.Join(currentTarget.transform.DOScale(targetStartScale, scaleBackDuration).SetEase(Ease.InOutQuad));

        // Hiệu ứng màu
        var playerRenderer = player.GetComponent<SpriteRenderer>();
        var targetRenderer = currentTarget.GetComponent<SpriteRenderer>();

        if (playerRenderer != null)
            s.Join(playerRenderer.DOColor(Color.green, baseDuration * 0.5f));

        if (targetRenderer != null)
            s.Join(targetRenderer.DOColor(Color.white, baseDuration * 0.5f));

        s.OnComplete(() =>
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;

            // SWAP hướng nhìn giữa player và target
            Vector3 tempScale = player.localScale;
            player.localScale = currentTarget.transform.localScale;
            currentTarget.transform.localScale = tempScale;

            // Nếu player có controller giữ biến m_FacingRight thì sync lại
            var controller = player.GetComponent<CharacterController2D>(); // tên class controller của bạn
            if (controller != null)
                controller.SyncFacingDirection(); // bạn sẽ tạo hàm này bên dưới

            if (playerAnimator != null) playerAnimator.speed = 1f;
            isSwapping = false;
        });

    }
}