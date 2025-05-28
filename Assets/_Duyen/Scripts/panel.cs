using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class panel : MonoBehaviour
{
    public GameObject panelSignIn;
    public GameObject panelSignUp;

    private float slideDistance = 1000f; // khoảng cách lướt, có thể điều chỉnh
    private float duration = 0.35f;

    private void Start()
    {
        // Đặt panel inactive và ra khỏi màn hình nếu cần
        panelSignIn.SetActive(true);
        panelSignUp.SetActive(false);
    }

    public void ShowSignIn()
    {
        AudioManager.Instance.PlayClickSFX();

        if (panelSignUp.activeSelf)
        {
            // Trượt panelSignUp xuống và ẩn
            panelSignUp.transform.DOLocalMoveY(-slideDistance, duration).SetEase(Ease.InBack)
                .OnComplete(() => panelSignUp.SetActive(false));
        }

        if (!panelSignIn.activeSelf)
        {
            panelSignIn.SetActive(true);
            RectTransform signInRT = panelSignIn.GetComponent<RectTransform>();
            signInRT.anchoredPosition = new Vector2(0, -slideDistance);
            signInRT.DOLocalMoveY(0, duration).SetEase(Ease.OutBack);
        }
    }

    public void ShowSignUp()
    {
        AudioManager.Instance.PlayClickSFX();

        if (panelSignIn.activeSelf)
        {
            // Trượt panelSignIn xuống và ẩn
            panelSignIn.transform.DOLocalMoveY(-slideDistance, duration).SetEase(Ease.InBack)
                .OnComplete(() => panelSignIn.SetActive(false));
        }

        if (!panelSignUp.activeSelf)
        {
            panelSignUp.SetActive(true);
            RectTransform signUpRT = panelSignUp.GetComponent<RectTransform>();
            signUpRT.anchoredPosition = new Vector2(0, -slideDistance);
            signUpRT.DOLocalMoveY(0, duration).SetEase(Ease.OutBack);
        }
    }
}
