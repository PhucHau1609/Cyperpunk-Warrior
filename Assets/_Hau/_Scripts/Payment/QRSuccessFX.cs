using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class QRSuccessFX : MonoBehaviour
{
    [Header("Refs (Success UI)")]
    public GameObject[] successUI;    // FlashOverlay, Checkmark, ConfettiRoot...
    public RectTransform qrRect;
    public RawImage qrImage;
    public Image flashOverlay;
    public Image checkmark;
    public ParticleSystem confetti;
    public QRPerimeterRunner runner;  // viền xanh chạy quanh (không thuộc success UI)
    public AudioSource sfx;

    [Header("Tune")]
    public float flashAlpha = 0.5f;
    public float punch = 0.08f;
    public float checkShowTime = 0.35f;
    public bool autoHideAfter = true;     // tắt success UI sau anim

    private Sequence seq;

    void Awake() { HideSuccessUI(); }
    void OnEnable() { HideSuccessUI(); }
    void OnDisable() { if (seq != null) seq.Kill(); }

    void HideSuccessUI()
    {
        if (successUI == null) return;
        foreach (var go in successUI) if (go) go.SetActive(false);

        if (flashOverlay) flashOverlay.color = new Color(1, 1, 1, 0);
        if (checkmark) checkmark.color = new Color(checkmark.color.r, checkmark.color.g, checkmark.color.b, 0);
        if (checkmark) checkmark.transform.localScale = Vector3.zero;
    }

    void ShowSuccessUI()
    {
        if (successUI == null) return;
        foreach (var go in successUI) if (go) go.SetActive(true);
    }

    public void PlaySuccess()
    {
        ShowSuccessUI();                 // ← chỉ bật khi success
        Debug.Log("Start All Anim");

        if (runner) runner.enabled = false;

        if (seq != null) seq.Kill();
        seq = DOTween.Sequence().SetLink(gameObject);

        // 1) Flash
        if (flashOverlay)
        {
            flashOverlay.color = new Color(1, 1, 1, 0);
            seq.Append(flashOverlay.DOFade(flashAlpha, 0.08f));
            seq.Append(flashOverlay.DOFade(0, 0.20f));
        }

        // 2) Punch QR
        if (qrRect)
            seq.Join(qrRect.DOPunchScale(new Vector3(punch, punch, 0), 0.45f, 8, 0.7f));

        // 3) Checkmark
        if (checkmark)
        {
            checkmark.transform.localScale = Vector3.zero;
            checkmark.color = new Color(checkmark.color.r, checkmark.color.g, checkmark.color.b, 0);
            seq.Insert(0.05f, checkmark.DOFade(1, 0.12f));
            seq.Insert(0.05f, checkmark.transform.DOScale(1f, checkShowTime).SetEase(Ease.OutBack));
            seq.AppendInterval(0.6f);
            seq.Append(checkmark.DOFade(0, 0.25f));
        }

        //// 4) Confetti + SFX
        //if (confetti) confetti.Play();
        //if (sfx) sfx.Play();

        Debug.Log("Done All Anim");

        if (autoHideAfter)
            seq.OnComplete(HideSuccessUI);
    }

    public void PlayFailed()
    {
        // vẫn ẩn success UI
        HideSuccessUI();

        if (runner) runner.enabled = false;
        if (seq != null) seq.Kill();

        if (qrRect) qrRect.DOShakeRotation(0.4f, 8f);
        if (flashOverlay)
        {
            ShowSuccessUI(); // cần active overlay để nhìn thấy flash đỏ
            flashOverlay.color = new Color(1, 0, 0, 0);
            flashOverlay.DOFade(0.35f, 0.06f)
                .OnComplete(() => flashOverlay.DOFade(0, 0.18f)
                .OnComplete(() => { if (autoHideAfter) HideSuccessUI(); }));
        }
    }
}
