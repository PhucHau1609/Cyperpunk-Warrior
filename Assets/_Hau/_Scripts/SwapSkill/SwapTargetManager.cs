using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;


public class SwapTargetManager : MonoBehaviour
{
    public static SwapTargetManager Instance;

    public Transform player;
    private SwapableObject currentTarget;
    private bool isSwapping = false;

    [Header("Swap Settings")]
    public float swapDuration = 0.3f;
    public Ease swapEase = Ease.InOutSine;
    public float swapSpeedMultiplier = 1.0f;
    public float slowMotionScale = 0.1f;

    private Animator playerAnimator;

    // NEW: reference đạn đặc biệt
    private Transform specialBullet;
    private CharacterController2D controller;

    private float lastSwapTime = -Mathf.Infinity;
    public float swapCooldown = 5f; // Thời gian cooldown (phù hợp với UI)
    public float maxSwapDistance = 5f; // Có thể cho inspector chỉnh nếu muốn

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //Debug.Log($"[SwapTargetManager] Scene loaded: {scene.name}, reset current target.");
        currentTarget = null;
    }



    void Awake()
    {
        Instance = this;
        //Debug.Log($"[SwapTargetManager] Set Instance from: {gameObject.name}");

        if (player != null) playerAnimator = player.GetComponentInChildren<Animator>();
        if (controller == null) controller = player.GetComponent<CharacterController2D>();
    }

    public bool ActiveSwapSkill()
    {
        if (Time.time < lastSwapTime + swapCooldown)
        {
            //Debug.Log("Swap skill đang trong thời gian hồi chiêu!");
            return false;
        }

        if (!isSwapping && PlayerStatus.Instance != null)
        {
            PlayerStatus.Instance.TriggerBlink(PlayerStatus.Instance.rImage);

            // 1. Swap với object bình thường
            if (currentTarget != null)
            {
                float distance = Vector3.Distance(player.position, currentTarget.transform.position);
                if (distance > maxSwapDistance)
                {
                    //Debug.Log("Khoảng cách quá xa, không thể swap!");
                    return false;
                }

                SwapWithEffect(currentTarget.transform, swapDuration);
                PlayerStatus.Instance.UseEnergy(10f);
                lastSwapTime = Time.time;
                return true;
            }

            // 2. Swap với đạn đặc biệt (không kiểm tra khoảng cách)
            else if (specialBullet != null)
            {
                SwapWithEffect(specialBullet, 0.1f, true);
                specialBullet = null;
                lastSwapTime = Time.time;
                return true;
            }
        }

        return false;
    }
    public void SetSpecialBullet(Transform bullet)
    {
        specialBullet = bullet;
    }

    public void SetTarget(SwapableObject target, bool selected)
    {
        //Debug.Log($"[SetTarget] target: {target.name}, selected: {selected}");

        if (selected)
        {
            if (currentTarget != null && currentTarget != target)
            {
                //Debug.Log($"[SetTarget] Deselect old target: {currentTarget.name}");
                currentTarget.isSelected = false;
                currentTarget.GetComponent<SpriteRenderer>().color = Color.white;
            }

            currentTarget = target;
            //Debug.Log($"[SetTarget] New current target: {currentTarget.name}");
        }
        else
        {
            if (currentTarget == target)
            {
                //Debug.Log($"[SetTarget] Unselect current target: {currentTarget.name}");
                currentTarget = null;
            }
        }
    }


    void SwapWithEffect(Transform targetTransform, float swapDurationOverride, bool destroyAfterSwap = false)
    {
        if (targetTransform == null || player == null) return;

        isSwapping = true;
        controller.invincible = true;

        Vector3 playerStartPos = player.position;
        Vector3 targetStartPos = targetTransform.position;

        Vector3 playerStartScale = player.localScale;
        Vector3 targetStartScale = targetTransform.localScale;

        float baseDuration = swapDurationOverride / Mathf.Max(swapSpeedMultiplier, 0.01f);
        float scaleDuration = baseDuration * 0.3f;
        float moveDuration = baseDuration * 0.4f;
        float scaleBackDuration = baseDuration * 0.3f;

        Time.timeScale = slowMotionScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        if (playerAnimator != null) playerAnimator.speed = 0f;

        Vector3 playerShrinkScale = playerStartScale * 0.4f;
        Vector3 targetShrinkScale = targetStartScale * 0.4f;

        Sequence s = DOTween.Sequence();

        s.Append(player.DOScale(playerShrinkScale, scaleDuration).SetEase(Ease.InOutQuad));
        s.Join(targetTransform.DOScale(targetShrinkScale, scaleDuration).SetEase(Ease.InOutQuad));

        s.Append(player.DOMove(targetStartPos, moveDuration).SetEase(swapEase));
        s.Join(targetTransform.DOMove(playerStartPos, moveDuration).SetEase(swapEase));

        s.Append(player.DOScale(playerStartScale, scaleBackDuration).SetEase(Ease.InOutQuad));
        s.Join(targetTransform.DOScale(targetStartScale, scaleBackDuration).SetEase(Ease.InOutQuad));

        var playerRenderer = player.GetComponent<SpriteRenderer>();
        var targetRenderer = targetTransform.GetComponent<SpriteRenderer>();

        if (playerRenderer != null)
            s.Join(playerRenderer.DOColor(Color.green, baseDuration * 0.5f));

        if (targetRenderer != null)
            s.Join(targetRenderer.DOColor(Color.white, baseDuration * 0.5f));

        s.OnComplete(() =>
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;

            Vector3 tempScale = player.localScale;
            player.localScale = targetTransform.localScale;
            targetTransform.localScale = tempScale;

            //var controller = player.GetComponent<CharacterController2D>();
            if (controller != null)
                controller.SyncFacingDirection();

            if (playerAnimator != null) playerAnimator.speed = 1f;
            isSwapping = false;
            controller.invincible = false;

            // NEW: Destroy đạn sau swap nếu cần
            if (destroyAfterSwap)
            {
                Destroy(targetTransform.gameObject);
            }
        });
    }
}



/*    public bool ActiveSwapSkill()
    {
        if (Time.time < lastSwapTime + swapCooldown)
        {
            Debug.Log("Swap skill đang trong thời gian hồi chiêu!");
            return false;
        }

        if (!isSwapping && PlayerStatus.Instance != null)
        {
            PlayerStatus.Instance.TriggerBlink(PlayerStatus.Instance.rImage);

            if (currentTarget != null)
            {
                SwapWithEffect(currentTarget.transform, swapDuration);
                PlayerStatus.Instance.UseEnergy(10f);
                lastSwapTime = Time.time; // Cập nhật thời gian sử dụng
                return true;
            }
            else if (specialBullet != null)
            {
                SwapWithEffect(specialBullet, 0.1f, true);
                specialBullet = null;
                lastSwapTime = Time.time; // Cập nhật thời gian sử dụng
                return true;
            }
        }

        return false;
    }
*/