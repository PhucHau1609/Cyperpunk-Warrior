using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq;
using TMPro;

// Updated CraftingUI để work với structure mới
public class CraftingUI : HauSingleton<CraftingUI>
{
    [Header("UI Tweening")]
    [SerializeField] protected Transform showHide;
    [SerializeField] protected Vector3 appearPosition = new Vector3(300f, 0f, 0f);
    [SerializeField] protected Vector3 hiddenPosition = new Vector3(1000f, 0f, 0f);

    [Header("Crafting Logic")]
    [SerializeField] protected CraftingSlot[] craftingSlots;
    [SerializeField] protected Button btnCraft;

    [Header("Button Color Settings")]
    [SerializeField] private TextMeshProUGUI craftButtonText;
    [SerializeField] private Color normalTextColor = Color.white;
    [SerializeField] private Color validRecipeColor = Color.green;

    public bool isShowUI = false;

    protected override void Start()
    {
        base.Start();
        this.showHide.localPosition = hiddenPosition;
        this.showHide.gameObject.SetActive(false);

        if (btnCraft != null)
            btnCraft.onClick.AddListener(OnClickCraft);

        if (craftButtonText == null && btnCraft != null)
            craftButtonText = btnCraft.GetComponentInChildren<TextMeshProUGUI>();

        CraftingSlot.OnSlotChanged += OnCraftingSlotsChanged;
        CheckRecipeValidity();
    }

    protected virtual void OnDestroy()
    {
        //base.OnDestroy();
        CraftingSlot.OnSlotChanged -= OnCraftingSlotsChanged;
    }

    private void OnCraftingSlotsChanged()
    {
        CheckRecipeValidity();
    }

    private void CheckRecipeValidity()
    {
        bool isValid = IsCurrentRecipeValid();
        UpdateButtonAppearance(isValid);
    }

    private bool IsCurrentRecipeValid()
    {
        ItemCode[] inputCodes = craftingSlots
            .Where(slot => slot.currentItem != null && slot.currentItem.ItemProfileSO != null)
            .Select(slot => slot.currentItem.ItemProfileSO.itemCode)
            .ToArray();

        if (inputCodes.Length == 0) return false;

        var recipe = CraftingManager.Instance.FindMatchingRecipe(inputCodes);
        return recipe != null;
    }

    private void UpdateButtonAppearance(bool isValid)
    {
        if (craftButtonText != null)
        {
            craftButtonText.color = isValid ? validRecipeColor : normalTextColor;
        }

        if (btnCraft != null)
        {
            btnCraft.interactable = isValid;
        }
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
        CheckRecipeValidity();
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
        if (!IsCurrentRecipeValid())
        {
            Debug.LogWarning("❌ Sai công thức hoặc thiếu nguyên liệu.");
            return;
        }

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

        // ✅ Sử dụng outputItemCode và outputItemCount
        if (recipe.outputItemCode != ItemCode.NoName)
        {
            InventoryManager.Instance.AddItem(recipe.outputItemCode, recipe.outputItemCount);
        }

        // Xóa UI Slot
        foreach (var slot in craftingSlots)
            slot.Clear();

        ObserverManager.Instance?.PostEvent(EventID.InventoryChanged);
        HauSoundManager.Instance.SpawnSound(Vector3.zero, SoundName.CraftItem);//Đổi sound craft
        // Log với recipe name nếu có
        string recipeName = !string.IsNullOrEmpty(recipe.recipeName) ? recipe.recipeName : recipe.outputItemCode.ToString();
        Debug.Log($"✅ Craft thành công: {recipeName} x{recipe.outputItemCount}");
    }
}