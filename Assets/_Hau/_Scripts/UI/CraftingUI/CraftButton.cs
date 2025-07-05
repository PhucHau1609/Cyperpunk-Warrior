using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
// 2. Tạo script cho Craft Button
public class CraftButton : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button craftButton;
    [SerializeField] private TextMeshProUGUI craftButtonText; // Hoặc TextMeshProUGUI nếu dùng TextMeshPro
    [SerializeField] private CraftingSlot[] craftingSlots; // Assign các crafting slots

    [Header("Text Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color validRecipeColor = Color.green; // Hoặc màu khác bạn muốn

    private void Start()
    {
        // Setup references nếu chưa assign
        SetupReferences();

        // Subscribe to slot change events
        CraftingSlot.OnSlotChanged += OnCraftingSlotsChanged;

        // Initial check
        CheckRecipeValidity();
    }

    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        CraftingSlot.OnSlotChanged -= OnCraftingSlotsChanged;
    }

    private void SetupReferences()
    {
        // Auto-setup nếu chưa assign
        if (craftButton == null)
            craftButton = GetComponent<Button>();

        if (craftButtonText == null)
            craftButtonText = GetComponentInChildren<TextMeshProUGUI>();

        if (craftingSlots == null || craftingSlots.Length == 0)
        {
            // Tự động tìm các crafting slots
            craftingSlots = FindObjectsByType<CraftingSlot>(FindObjectsSortMode.None);
        }
    }

    private void OnCraftingSlotsChanged()
    {
        CheckRecipeValidity();
    }

    private void CheckRecipeValidity()
    {
        bool isValidRecipe = IsCurrentRecipeValid();
        UpdateButtonAppearance(isValidRecipe);
    }

    private bool IsCurrentRecipeValid()
    {
        // Lấy items hiện tại từ các slots
        List<ItemCode> currentItems = new List<ItemCode>();

        foreach (CraftingSlot slot in craftingSlots)
        {
            if (slot.HasItem())
            {
                currentItems.Add(slot.currentItem.ItemProfileSO.itemCode);
            }
        }

        // Nếu không có item nào thì không valid
        if (currentItems.Count == 0)
            return false;

        // Check với CraftingManager
        CraftingRecipe matchingRecipe = CraftingManager.Instance.FindMatchingRecipe(currentItems.ToArray());
        return matchingRecipe != null;
    }

    private void UpdateButtonAppearance(bool isValid)
    {
        if (craftButtonText != null)
        {
            craftButtonText.color = isValid ? validRecipeColor : normalColor;
        }

        // Optional: Cũng có thể thay đổi interactable state
        if (craftButton != null)
        {
            craftButton.interactable = isValid;
        }
    }

    // Method để gọi khi nhấn craft button
    public void OnCraftButtonClicked()
    {
        if (!IsCurrentRecipeValid())
        {
            Debug.Log("Invalid recipe!");
            return;
        }

        // Thực hiện crafting logic
        PerformCrafting();
    }

    private void PerformCrafting()
    {
        // Lấy items hiện tại
        List<ItemCode> currentItems = new List<ItemCode>();
        foreach (CraftingSlot slot in craftingSlots)
        {
            if (slot.HasItem())
            {
                currentItems.Add(slot.currentItem.ItemProfileSO.itemCode);
            }
        }

        // Tìm recipe
        CraftingRecipe recipe = CraftingManager.Instance.FindMatchingRecipe(currentItems.ToArray());
        if (recipe == null) return;

        // Clear các slots
        foreach (CraftingSlot slot in craftingSlots)
        {
            slot.Clear();
        }

        // Thêm item đã craft vào inventory (sử dụng outputItemCode thay vì outputItem)
        InventoryManager.Instance.AddItem(recipe.outputItemCode, recipe.outputItemCount);
        ObserverManager.Instance.PostEvent(EventID.InventoryChanged);

        Debug.Log($"Crafted: {recipe.outputItemCode} x{recipe.outputItemCount}");
    }
}
