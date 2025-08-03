using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnergyCoreInventoryUI : HauSingleton<EnergyCoreInventoryUI>
{
    [SerializeField] private Transform contentHolder; // nơi chứa các button
    [SerializeField] private BtnItemInventory defaultBtn;
    private Dictionary<int, BtnItemInventory> btnDict = new();

    protected override void Start()
    {
        this.defaultBtn.gameObject.SetActive(false);
        this.HideUI();
    }

    public void ShowUI()
    {
        if (GameStateManager.Instance.CurrentState == GameState.Inventory) return;
        if (GameStateManager.Instance.CurrentState == GameState.Crafting) return;

        //
        this.gameObject.SetActive(true);
        GameStateManager.Instance.SetState(GameState.MiniGame);
        UpdateUI();
    }

    public void HideUI()
    {
        GameStateManager.Instance.ResetToGameplay();
        this.gameObject.SetActive(false);
    }

    public void ToggleUI()
    {
        if (this.gameObject.activeSelf) HideUI();
        else ShowUI();
    }

    public void UpdateUI()
    {
        var allItems = InventoryManager.Instance.ItemInventory().ItemInventories;

        foreach (var item in allItems)
        {
            if (!IsEnergyCore(item.ItemProfileSO.itemCode)) continue;

            if (!btnDict.ContainsKey(item.ItemId))
            {
                BtnItemInventory newBtn = Instantiate(defaultBtn, contentHolder);
                newBtn.SetItem(item);
                newBtn.gameObject.SetActive(true);
                newBtn.name = "Energy_" + item.ItemId;
                btnDict.Add(item.ItemId, newBtn);
            }
        }

        // Dọn nút nếu item không còn trong inventory
        var toRemove = btnDict
            .Where(kv => !allItems.Any(i => i.ItemId == kv.Key && IsEnergyCore(i.ItemProfileSO.itemCode)))
            .Select(kv => kv.Key)
            .ToList();

        foreach (int id in toRemove)
        {
            Destroy(btnDict[id].gameObject);
            btnDict.Remove(id);
        }
    }

    private bool IsEnergyCore(ItemCode code)
    {
        // ✅ Toàn bộ energy core bạn xài là UpgradeItem_1,2,3...
        return code.ToString().StartsWith("UpgradeItem_");
    }
}
