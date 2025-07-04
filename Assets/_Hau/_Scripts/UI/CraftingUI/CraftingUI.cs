using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class CraftingUI : HauSingleton<CraftingUI>
{
    [Header("UI Tweening")]
    [SerializeField] protected Transform showHide;
    [SerializeField] protected Vector3 appearPosition = new Vector3(300f, 0f, 0f);
    [SerializeField] protected Vector3 hiddenPosition = new Vector3(1000f, 0f, 0f);

    [Header("Crafting Logic")]
    [SerializeField] protected CraftingSlot[] craftingSlots;
    [SerializeField] protected Button btnCraft;

    protected bool isShowUI = false;

    protected override void Start()
    {
        base.Start();
        this.showHide.localPosition = hiddenPosition;
        this.showHide.gameObject.SetActive(false);

        if (btnCraft != null)
            btnCraft.onClick.AddListener(OnClickCraft);
    }

    public virtual void Toggle()
    {
        if (isShowUI) this.HideUI();
        else this.ShowUI();
    }

    public virtual void ShowUI()
    {
        isShowUI = true;
        this.showHide.gameObject.SetActive(true);
        this.showHide.DOLocalMove(appearPosition, 0.3f);
        NewInventoryUI.Instance.MoveToSide();
    }

    public virtual void HideUI()
    {
        isShowUI = false;
        this.showHide.DOLocalMove(hiddenPosition, 0.3f).OnComplete(() =>
        {
            this.showHide.gameObject.SetActive(false);
        });

        NewInventoryUI.Instance.MoveToCenter();
    }

    protected virtual void OnClickCraft()
    {
        ItemCode[] inputCodes = craftingSlots
            .Where(slot => slot.currentItem != null && slot.currentItem.ItemProfileSO != null)
            .Select(slot => slot.currentItem.ItemProfileSO.itemCode)
            .ToArray();

        var recipe = CraftingManager.Instance.FindMatchingRecipe(inputCodes);

        if (recipe == null)
        {
            Debug.LogWarning("❌ Sai công thức hoặc thiếu nguyên liệu.");
            return;
        }

        // ❌ KHÔNG cần trừ nguyên liệu ở đây nữa vì đã trừ khi drag
        // ✅ Chỉ cần thêm item mới vào inventory

        InventoryManager.Instance.AddItem(recipe.outputItem.itemCode, 1);

        // Xóa UI Slot
        foreach (var slot in craftingSlots)
            slot.Clear();

        // Gửi sự kiện cập nhật lại UI
        ObserverManager.Instance?.PostEvent(EventID.InventoryChanged);

        Debug.Log("✅ Craft thành công: " + recipe.outputItem.itemName);
    }

}


