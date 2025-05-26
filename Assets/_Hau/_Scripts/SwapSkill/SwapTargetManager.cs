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

    void Awake()
    {
        Instance = this;
    }

    public void SetTarget(SwapableObject target, bool selected)
    {
        if (selected)
        {
            // Bỏ đánh dấu target cũ nếu có
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

        Sequence s = DOTween.Sequence();

        // Tween vị trí
        s.Join(player.DOMove(targetStartPos, swapDuration).SetEase(swapEase));
        s.Join(currentTarget.transform.DOMove(playerStartPos, swapDuration).SetEase(swapEase));

        // Tween punch scale
        s.Join(player.DOPunchScale(Vector3.one * 0.2f, swapDuration, 6));
        s.Join(currentTarget.transform.DOPunchScale(Vector3.one * 0.2f, swapDuration, 6));

        // Flash màu
        var playerRenderer = player.GetComponent<SpriteRenderer>();
        var targetRenderer = currentTarget.GetComponent<SpriteRenderer>();

        if (playerRenderer != null)
        {
            s.Join(playerRenderer.DOColor(Color.white, 0.1f).OnComplete(() =>
                playerRenderer.DOColor(Color.green, 0.2f)));
        }

        if (targetRenderer != null)
        {
            s.Join(targetRenderer.DOColor(Color.white, 0.1f).OnComplete(() =>
                targetRenderer.DOColor(Color.white, 0.2f)));
        }

        s.OnComplete(() =>
        {
            FixFacingDirection(player);
            FixFacingDirection(currentTarget.transform);
            isSwapping = false;
        });
    }


    void FixFacingDirection(Transform obj)
    {
        Vector3 scale = obj.localScale;
        if (scale.x < 0) scale.x *= -1; // reset về phải
        obj.localScale = scale;
    }

}
