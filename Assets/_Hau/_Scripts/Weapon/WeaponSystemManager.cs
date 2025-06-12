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
            SwapWeapon();
        }

        if (Input.GetMouseButtonDown(1)) // Chuột phải để tắt vũ khí hiện tại
        {
            DeactivateCurrentWeapon();
        }
    }

    void RefreshOwnedWeapons()
    {
        ownedWeapons.Clear();
        var inventory = InventoryManager.Instance.ItemInventory().ItemInventories;

        foreach (var item in inventory)
        {
            if (item.ItemProfileSO == null) continue;

            if (item.ItemProfileSO.weaponType == WeaponType.Gun)
            {
                ownedWeapons.Add(item);
            }
        }
    }

    void SwapWeapon()
    {
        RefreshOwnedWeapons();

        if (ownedWeapons.Count == 0)
        {
            Debug.Log("Không có vũ khí nào trong inventory.");
            return;
        }

        currentWeaponIndex++;
        if (currentWeaponIndex >= ownedWeapons.Count)
        {
            currentWeaponIndex = 0; // Vòng lại vũ khí đầu tiên
        }

        EquipWeapon(currentWeaponIndex);
    }

    void DeactivateCurrentWeapon()
    {
        if (currentWeapon != null)
        {
            currentWeapon.SetActive(false);
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

        // Nếu đã có vũ khí hiện tại và khác với weapon mới → huỷ vũ khí cũ
        if (currentWeapon != null)
        {
            Destroy(currentWeapon); // Xoá vũ khí cũ nếu cần
        }

        // Instantiate vũ khí mới
        currentWeapon = Instantiate(weaponPrefab, weaponHolder);
        currentWeapon.transform.localPosition = new Vector3(0.806f, 0f, 0f);
        currentWeapon.transform.localRotation = Quaternion.identity;
        currentWeapon.SetActive(true);

        // Gán cho WeaponAimer
        if (weaponHolder.TryGetComponent<WeaponAimer>(out var aimer))
        {
            aimer.SetCurrentWeapon(currentWeapon.transform);
        }
    }
}

