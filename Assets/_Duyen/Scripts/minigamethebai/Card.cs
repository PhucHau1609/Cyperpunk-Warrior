using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Card : MonoBehaviour
{
    [SerializeField] private Image iconImage;

    public Sprite hiddenIconSprite;
    public Sprite iconSprite;

    public bool isSelected;
    public bool isMatched;

    public CardsController controller;

    public void OnCardClick()
    {
        if (!isMatched)
            controller.SetSelected(this);
    }

    public void SetIconSprite(Sprite sp)
    {
        iconSprite = sp;
        iconImage.sprite = hiddenIconSprite;
        isSelected = false;
        isMatched = false;
    }

    public void Show()
    {
        transform.DORotate(new Vector3(0f, 180f, 0f), .2f);
        DOVirtual.DelayedCall(0.1f, () => iconImage.sprite = iconSprite);
        isSelected = true;
    }

    public void Hide()
    {
        transform.DORotate(new Vector3(0f, 0f, 0f), .2f);
        DOVirtual.DelayedCall(0.1f, () =>
        {
            iconImage.sprite = hiddenIconSprite;
            isSelected = false;
        });
    }

    public void Match()
    {
        isMatched = true;
    }

    public void FadeOut()
    {
        isMatched = true;
        isSelected = true;

        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = gameObject.AddComponent<CanvasGroup>();
        }

        cg.DOFade(0f, 0.3f).SetEase(Ease.InOutSine);
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }

}
