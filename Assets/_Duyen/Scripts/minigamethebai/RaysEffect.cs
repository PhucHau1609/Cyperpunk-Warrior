using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class RaysEffect : MonoBehaviour
{
    public Image raysImage;

    void Start()
    {
        PlayRaysEffect();
    }

    public void PlayRaysEffect()
    {
        // Reset trạng thái ban đầu
        raysImage.transform.localScale = Vector3.one * 1f;
        raysImage.transform.rotation = Quaternion.identity;

        // Xoay mãi mãi
        raysImage.transform.DORotate(new Vector3(0, 0, 360f), 4f, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1);

        // Scale phóng/thu liên tục
        raysImage.transform.DOScale(1.2f, 1f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }
}
