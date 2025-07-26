using UnityEngine;
using DG.Tweening;

public class PlayerAppearanceUI : HauSingleton<PlayerAppearanceUI>
{
    [SerializeField] protected Transform showHide;
    [SerializeField] protected Vector3 appearPosition = new Vector3(300f, 0f, 0f);
    [SerializeField] protected Vector3 hiddenPosition = new Vector3(1000f, 0f, 0f);

    public bool isShowUI = false;

    public void ShowUI()
    {
        isShowUI = true;
        showHide.gameObject.SetActive(true);
        showHide.DOLocalMove(appearPosition, 0.3f);
    }

    public void HideUI()
    {
        isShowUI = false;
        showHide.DOLocalMove(hiddenPosition, 0.3f).OnComplete(() =>
        {
            showHide.gameObject.SetActive(false);
        });
    }
}
