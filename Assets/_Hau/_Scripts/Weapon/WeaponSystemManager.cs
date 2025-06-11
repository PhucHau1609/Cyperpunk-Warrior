using com.cyborgAssets.internalIBP;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSystemManager : MonoBehaviour
{
    [SerializeField] private Transform weaponHolder; // Nơi gắn vũ khí
    private GameObject currentWeapon;

    private int currentWeaponIndex = 0;
    private List<ItemInventory> ownedWeapons = new();

    void Start()
    {
        RefreshOwnedWeapons();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            ToggleWeapon();
        }
    }

    // Lọc các vũ khí trong inventory
    void RefreshOwnedWeapons()
    {
        ownedWeapons.Clear();

        var inventory = InventoryManager.Instance.ItemInventory().ItemInventories;

        foreach (var item in inventory)
        {
            if (item.ItemProfileSO == null) continue;

            // Giả định: bạn gắn tag "Gun" hoặc có enum TypeWeapon nếu cần phân biệt rõ hơn
            if (item.ItemProfileSO.itemCode == ItemCode.MachineGun_0) // <- có thể thay đổi tùy theo enum/tag bạn dùng
            {
                ownedWeapons.Add(item);
            }
        }
    }

    void ToggleWeapon()
    {
        RefreshOwnedWeapons();

        if (ownedWeapons.Count == 0)
        {
            Debug.Log("Không có vũ khí nào trong inventory.");
            return;
        }

        if (currentWeapon == null)
        {
            EquipWeapon(currentWeaponIndex);
        }
        else
        {
            currentWeapon.SetActive(!currentWeapon.activeSelf);
        }
    }

    void EquipWeapon(int index)
    {
        if (index < 0 || index >= ownedWeapons.Count) return;

        var item = ownedWeapons[index];
        var weaponPrefab = item.ItemProfileSO.prefabItem;

        if (weaponPrefab == null)
        {
            Debug.LogWarning("Vũ khí không có prefab.");
            return;
        }

        if (currentWeapon == null)
        {
            currentWeapon = Instantiate(weaponPrefab);
            currentWeapon.transform.SetParent(weaponHolder, false);
            currentWeapon.transform.localPosition = new Vector3(0.806f, 0f, 0f);
            currentWeapon.transform.localRotation = Quaternion.identity;
        }

        currentWeapon.SetActive(true);
    }

}
