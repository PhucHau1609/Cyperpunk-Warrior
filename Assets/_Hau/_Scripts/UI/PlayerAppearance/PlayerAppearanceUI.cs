using UnityEngine;
using DG.Tweening;
using NUnit.Framework;
using System.Collections.Generic;

public class PlayerAppearanceUI : HauSingleton<PlayerAppearanceUI>
{
    [Header("Position UI")]
    [SerializeField] protected Transform showHide;
    [SerializeField] protected Vector3 appearPosition = new Vector3(300f, 0f, 0f);
    [SerializeField] protected Vector3 hiddenPosition = new Vector3(1000f, 0f, 0f);


    [Header("Equiqment Slot")]
    [SerializeField] protected List<EquipmentSlot> slot;


    public bool isShowUI = false;

    protected override void ResetValue()
    {
        base.ResetValue();
        this.appearPosition = new Vector3(520f, 0f, 0f);
    }

    protected override void LoadComponents()
    {
        base.LoadComponents();
        this.LoadEquipmentSlot();
        this.LoadShowHide();
    }

    protected virtual void LoadShowHide()
    {
        if (this.showHide != null) return;
        this.showHide = transform.Find("ShowHide");
        Debug.LogWarning(transform.name + ": LoadShowHide: " + gameObject);
    }

    protected virtual void LoadEquipmentSlot()
    {
        if (this.slot.Count > 0) return;

        foreach (Transform child in transform.Find("ShowHide/Appearance/Scroll View/Viewport/ContentEquiment"))
        {
            EquipmentSlot equimentSlot = child.GetComponentInChildren<EquipmentSlot>();
            if (equimentSlot == null) continue;
            this.slot.Add(equimentSlot);
        }

        Debug.LogWarning(transform.name + ": LoadEquipmentSlot", gameObject);
    }    

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
