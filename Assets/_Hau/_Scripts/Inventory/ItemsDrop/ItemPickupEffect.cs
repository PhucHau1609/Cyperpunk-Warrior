using UnityEngine;
using DG.Tweening;

public class ItemPickupEffect : MonoBehaviour
{
    private Material material;
    private Tween alphaTween;
    private Tween scaleTween;
    private Tween moveTween;

    void Start()
    {
        // Lấy material (gán từ SpriteRenderer)
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        material = sr.material;

        // Tween nhấp nháy alpha (0.3 ~ 1)
        alphaTween = DOTween
            .To(() => material.color.a,
                x => {
                    Color c = material.color;
                    c.a = x;
                    material.color = c;
                },
                0.3f, 1f)
            .SetLoops(-1, LoopType.Yoyo);

        // Tween scale (1 ~ 1.15)
        scaleTween = transform
            .DOScale(1.3f, 0.6f)
            .SetLoops(-1, LoopType.Yoyo);

        // Tween nhún lên – xuống (pos.y +/- 0.05)
        moveTween = transform
            .DOMoveY(transform.position.y + 0.05f, 0.6f)
            .SetLoops(-1, LoopType.Yoyo);
    }

    void OnDisable()
    {
        // Kill tween khi object bị disable
        alphaTween?.Kill();
        scaleTween?.Kill();
        moveTween?.Kill();
    }
}
