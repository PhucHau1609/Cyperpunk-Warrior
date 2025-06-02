using System.Collections;
using UnityEngine;
using DG.Tweening;

public class panel : MonoBehaviour
{
    public GameObject panelSignIn;
    public GameObject panelSignUp;

    private float duration = 0.35f;
    private Vector3 hiddenScale = Vector3.zero;
    private Vector3 shownScale = Vector3.one;

    private void Start()
    {
        // Đặt trạng thái ban đầu
        panelSignIn.transform.localScale = shownScale;
        panelSignIn.SetActive(true);

        panelSignUp.transform.localScale = hiddenScale;
        panelSignUp.SetActive(false);
    }

    public void ShowSignIn()
    {
        AudioManager.Instance.PlayClickSFX();

        if (panelSignUp.activeSelf)
        {
            panelSignUp.transform.DOScale(hiddenScale, duration).SetEase(Ease.InBack)
                .OnComplete(() => panelSignUp.SetActive(false));
        }

        if (!panelSignIn.activeSelf)
        {
            panelSignIn.SetActive(true);
            panelSignIn.transform.localScale = hiddenScale;
            panelSignIn.transform.DOScale(shownScale, duration).SetEase(Ease.OutBack);
        }
    }

    public void ShowSignUp()
    {
        AudioManager.Instance.PlayClickSFX();

        if (panelSignIn.activeSelf)
        {
            panelSignIn.transform.DOScale(hiddenScale, duration).SetEase(Ease.InBack)
                .OnComplete(() => panelSignIn.SetActive(false));
        }

        if (!panelSignUp.activeSelf)
        {
            panelSignUp.SetActive(true);
            panelSignUp.transform.localScale = hiddenScale;
            panelSignUp.transform.DOScale(shownScale, duration).SetEase(Ease.OutBack);
        }
    }
}
