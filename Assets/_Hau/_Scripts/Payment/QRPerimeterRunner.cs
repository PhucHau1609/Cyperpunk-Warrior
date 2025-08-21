using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Chạy 1 Image nhỏ quanh viền của QR (hoặc bất kỳ RectTransform nào).
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class QRPerimeterRunner : MonoBehaviour
{
    [Header("Targets")]
    public RectTransform qrRect;       // = qrImage.rectTransform
    public RectTransform runner;       // = Image xanh lá

    [Header("Motion")]
    public float speed = 420f;         // pixel/giây
    public bool clockwise = true;
    [Tooltip("Lùi vào trong viền (pixel) để không đè lên QR.")]
    public float inset = 6f;

    private Sequence seq;

    private void Reset()
    {
        qrRect = GetComponent<RectTransform>();
    }

    private void OnEnable() => BuildPath();
    private void OnDisable() { if (seq != null) seq.Kill(); }

    /// Gọi lại khi bạn vừa Generate xong QR hoặc khi đổi kích thước.
    public void Refresh() => BuildPath();

    private void OnRectTransformDimensionsChange()
    {
        // Nếu QR thay đổi kích thước khi layout chạy, rebuild path.
        if (isActiveAndEnabled) BuildPath();
    }

    private void BuildPath()
    {
        if (!qrRect || !runner) return;

        if (seq != null) seq.Kill();

        // runner chạy trong local-space của qrRect
        runner.SetParent(qrRect, worldPositionStays: false);

        // Tính 4 góc (local) đã inset
        Vector2 half = (qrRect.rect.size / 2f) - new Vector2(inset, inset);
        Vector3[] corners = new Vector3[4];
        corners[0] = new Vector3(-half.x, half.y, 0); // TL
        corners[1] = new Vector3(half.x, half.y, 0); // TR
        corners[2] = new Vector3(half.x, -half.y, 0); // BR
        corners[3] = new Vector3(-half.x, -half.y, 0); // BL
        if (!clockwise) System.Array.Reverse(corners);

        runner.anchoredPosition = (Vector2)corners[0];

        seq = DOTween.Sequence().SetAutoKill(false).SetLink(gameObject);

        // Nối 4 cạnh: TL→TR→BR→BL→TL…
        for (int i = 0; i < 4; i++)
        {
            Vector3 a = corners[i];
            Vector3 b = corners[(i + 1) % 4];

            float dist = Vector3.Distance(a, b);
            float dur = dist / Mathf.Max(1f, speed);

            // Xoay runner theo hướng cạnh (chỉ để đẹp mắt)
            Vector3 dir = (b - a).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            seq.Append(
                runner.DOAnchorPos((Vector2)b, dur)
                      .SetEase(Ease.Linear)
            );
            seq.Join(
                runner.DOLocalRotate(new Vector3(0, 0, angle), 0.08f)
            );
        }

        seq.SetLoops(-1, LoopType.Restart);
    }
}
