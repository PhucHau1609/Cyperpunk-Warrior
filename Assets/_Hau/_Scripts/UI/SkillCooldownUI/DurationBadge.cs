using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;
using System;

public class DurationBadge : MonoBehaviour
{
    [Header("Refs")]
    public Image iconImage;
    public Image durationRing;          // Image Filled Radial
    public TextMeshProUGUI secondsText; // optional

    private Tween ringTween;
    private Coroutine textCo;
    private Action _onFinish;

    public void Setup(Sprite icon)
    {
        if (iconImage) iconImage.sprite = icon;
        if (durationRing) { durationRing.fillAmount = 0f; durationRing.gameObject.SetActive(false); }
        if (secondsText) { secondsText.gameObject.SetActive(false); }
    }

    public void StartTimer(float seconds, Action onFinish = null)
    {
        _onFinish = onFinish;
        // cleanup
        ringTween?.Kill();
        if (textCo != null) StopCoroutine(textCo);

        if (durationRing)
        {
            durationRing.gameObject.SetActive(true);
            durationRing.fillAmount = 1f;
            ringTween = durationRing.DOFillAmount(0f, seconds)
                                    .SetEase(Ease.Linear)
                                    .OnComplete(() => Finish());
        }

        if (secondsText)
        {
            secondsText.gameObject.SetActive(true);
            textCo = StartCoroutine(TextCountdown(seconds));
        }
    }

    private IEnumerator TextCountdown(float seconds)
    {
        float t = seconds;
        while (t > 0f)
        {
            secondsText.text = Mathf.CeilToInt(t).ToString();
            t -= Time.deltaTime;
            yield return null;
        }
        secondsText.gameObject.SetActive(false);
        textCo = null;
    }

    public void ForceStop()
    {
        ringTween?.Kill();
        if (durationRing) durationRing.gameObject.SetActive(false);
        if (textCo != null) { StopCoroutine(textCo); textCo = null; }
        if (secondsText) secondsText.gameObject.SetActive(false);
        _onFinish?.Invoke();
        _onFinish = null;
    }

    private void Finish()
    {
        if (durationRing) durationRing.gameObject.SetActive(false);
        if (secondsText) secondsText.gameObject.SetActive(false);
        _onFinish?.Invoke();
        _onFinish = null;
    }
}
