using TMPro;
using Unity.VisualScripting;
using UnityEngine;

using DG.Tweening;

public class ItemTooltipUI : HauSingleton<ItemTooltipUI>
{
    [Header("Tooltip UI")]
    [SerializeField] private GameObject tooltip1Small;
    [SerializeField] private GameObject tooltip2Large;

    [SerializeField] private CanvasGroup tooltip1Group;
    [SerializeField] private CanvasGroup tooltip2Group;

    [Header("Tooltip1 - Small UI")]
    [SerializeField] private TextMeshProUGUI txtName1;

    [Header("Tooltip2 - Large UI")]
    [SerializeField] private TextMeshProUGUI txtName2;
    [SerializeField] private TextMeshProUGUI txtDescription2;

    [Header("Vị trí hiển thị")]
    [SerializeField] private Vector3 posTooltip1 = new Vector3(300f, 0f, 0f);
    [SerializeField] private Vector3 posTooltip2 = new Vector3(150f, 200f, 0f);

    private float fadeDuration = 0.2f;

    protected override void ResetValue()
    {
        base.ResetValue();
        this.posTooltip1 = new Vector3 (670f, 200f, 0f);
        this.posTooltip2 = new Vector3 (500f, 230f, 0f);
    }

    protected override void Start()
    {
        HideTooltip();
    }

    public void ShowTooltip(ItemProfileSO itemProfile)
    {
        // Ẩn trước
        HideTooltipInstant(); // Không hiệu ứng, chỉ tắt trước khi hiện cái mới

        bool isCraftingOpen = CraftingUI.HasInstance && CraftingUI.Instance.isShowUI;

        if (isCraftingOpen)
        {
            tooltip2Large.SetActive(true);
            tooltip2Large.transform.localPosition = posTooltip2;
            txtName2.text = itemProfile.itemName;
            //txtDescription2.text = itemProfile.description;

            tooltip2Group.alpha = 0;
            tooltip2Group.DOFade(1, fadeDuration);
        }
        else
        {
            tooltip1Small.SetActive(true);
            tooltip1Small.transform.localPosition = posTooltip1;
            txtName1.text = itemProfile.itemName;

            tooltip1Group.alpha = 0;
            tooltip1Group.DOFade(1, fadeDuration);
        }
    }

    public void HideTooltip()
    {
        if (tooltip1Small.activeSelf)
        {
            tooltip1Group.DOFade(0, fadeDuration).OnComplete(() =>
            {
                tooltip1Small.SetActive(false);
            });
        }

        if (tooltip2Large.activeSelf)
        {
            tooltip2Group.DOFade(0, fadeDuration).OnComplete(() =>
            {
                tooltip2Large.SetActive(false);
            });
        }
    }

    private void HideTooltipInstant()
    {
        tooltip1Group.DOKill();
        tooltip2Group.DOKill();

        tooltip1Small.SetActive(false);
        tooltip2Large.SetActive(false);
    }
}



