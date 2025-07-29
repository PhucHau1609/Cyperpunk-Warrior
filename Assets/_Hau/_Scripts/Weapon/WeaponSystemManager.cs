using UnityEngine;
using System.Collections.Generic;

public class WeaponSystemManager : HauSingleton<WeaponSystemManager>
{
    public Transform weaponHolder;
    private GameObject currentWeapon;
    private int currentWeaponIndex = 0;
    public bool isWeaponActive = false;

    private List<ItemInventory> ownedWeapons = new();
    private Dictionary<int, GameObject> instantiatedWeapons = new(); // key: index
    private bool wasWeaponActiveBeforeWallSlide = false; // Lưu trạng thái súng trước khi wall slide
    private CharacterController2D characterController;


    protected override void Start()
    {
        RefreshOwnedWeapons();

        // Tìm CharacterController2D và đăng ký events
        characterController = FindFirstObjectByType<CharacterController2D>();
        if (characterController != null)
        {
            characterController.OnWallSlideStart.AddListener(HideWeaponDuringWallSlide);
            characterController.OnWallSlideEnd.AddListener(ShowWeaponAfterWallSlide);
        }
    }

    void Update()
    {
        if (characterController.isDead) return;

        if (Input.GetKeyDown(KeyCode.T))
        {
            SwapWeapon();
        }

        if (Input.GetMouseButtonDown(1))
        {
            ToggleWeapon();
        }
    }

    void RefreshOwnedWeapons()
    {
        ownedWeapons.Clear();
        var inventory = InventoryManager.Instance.ItemInventory().ItemInventories;

        for (int i = 0; i < inventory.Count; i++)
        {
            var item = inventory[i];
            if (item.ItemProfileSO == null) continue;

            if (item.ItemProfileSO.weaponType == WeaponType.Gun)
            {
                ownedWeapons.Add(item);
            }
        }
    }

    void ToggleWeapon()
    {
        if (ownedWeapons.Count == 0) return;

        if (currentWeapon == null)
        {
            EquipWeapon(currentWeaponIndex);
            isWeaponActive = true;
        }
        else
        {
            isWeaponActive = !isWeaponActive;
            currentWeapon.SetActive(isWeaponActive);
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

        int previousIndex = currentWeaponIndex;

        currentWeaponIndex++;
        if (currentWeaponIndex >= ownedWeapons.Count)
        {
            currentWeaponIndex = 0;
        }

        // Tắt vũ khí cũ nếu đang bật
        if (currentWeapon != null)
        {
            currentWeapon.SetActive(false);
        }

        EquipWeapon(currentWeaponIndex); // Giữ trạng thái isWeaponActive
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

        GameObject weaponInstance;

        // Nếu vũ khí đã từng instantiate rồi
        if (instantiatedWeapons.ContainsKey(index))
        {
            weaponInstance = instantiatedWeapons[index];
        }
        else
        {
            weaponInstance = Instantiate(weaponPrefab, weaponHolder);
            weaponInstance.transform.localPosition = new Vector3(0.806f, 0f, 0f);
            weaponInstance.transform.localRotation = Quaternion.identity;
            instantiatedWeapons.Add(index, weaponInstance);
        }

        currentWeapon = weaponInstance;
        currentWeapon.SetActive(isWeaponActive);

        // Gán cho WeaponAimer
        if (weaponHolder.TryGetComponent<WeaponAimer>(out var aimer))
        {
            aimer.SetCurrentWeapon(currentWeapon.transform);
        }
    }


    // Phương thức ẩn súng khi wall sliding
    public void HideWeaponDuringWallSlide()
    {
        if (currentWeapon != null && currentWeapon.activeSelf)
        {
            wasWeaponActiveBeforeWallSlide = true;
            currentWeapon.SetActive(false);
        }
        else
        {
            wasWeaponActiveBeforeWallSlide = false;
        }
    }

    // Phương thức hiện súng sau khi kết thúc wall sliding
    public void ShowWeaponAfterWallSlide()
    {
        if (currentWeapon != null && wasWeaponActiveBeforeWallSlide)
        {
            currentWeapon.SetActive(true);
            wasWeaponActiveBeforeWallSlide = false;
        }
    }

    public void TurnOffAllWeapon()
    {
        isWeaponActive = false;

        if (weaponHolder != null)
        {
            foreach (Transform child in weaponHolder)
            {
                child.gameObject.SetActive(false);
            }
        }
    }    
}