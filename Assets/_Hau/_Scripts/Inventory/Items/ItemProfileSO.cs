using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "ItemProfile", menuName = "ScriptableObjects/ItemProfile", order = 1)]
public class ItemProfileSO : ScriptableObject
{
    public InventoryCodeName invCodeName;
    public ItemCode itemCode;
    public WeaponType weaponType;
    public EquipmentType equipmentType = EquipmentType.None;
    public string itemName;
    public Sprite itemSprite;
    public bool isStackable = false;
    public bool canNotDelete = true;
    public GameObject prefabItem;

    [TextArea(2, 5)] public string itemDescription; // <-- THÊM DÒNG NÀY

    // ✅ THÊM DÒNG NÀY
    public ItemUseType useType = ItemUseType.None;
    public float healAmount = 10f;


    protected virtual void Reset()
    {
        this.ResetValue();
    }
    
    protected virtual void ResetValue()
    {
        this.AutoLoadItemCode();
        this.AutoLoadItemName();
        this.itemDescription = "Mô tả mặc định..."; // <-- Có thể thêm mặc định
    }

    protected virtual void AutoLoadItemCode()
    {
        string className = this.GetType().Name;
        Debug.Log("className: " + className);
        this.itemCode = ItemCodeParse.Parse("Item1");
    }

    protected virtual void AutoLoadItemName()
    {
        Debug.Log("name: " + this.name);
        this.itemName = "Item1";
    }
}
