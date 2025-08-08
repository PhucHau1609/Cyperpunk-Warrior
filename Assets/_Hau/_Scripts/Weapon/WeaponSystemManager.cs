using UnityEngine;
using System.Collections.Generic;

public class WeaponSystemManager : HauSingleton<WeaponSystemManager>
{
    public Transform weaponHolder;
    private GameObject currentWeapon;
    private int currentWeaponIndex = 0;
    public bool isWeaponActive = false;
    [Header("Equip Gun Slot")]
    [SerializeField] private EquipmentSlot[] gunSlots;


    private List<ItemInventory> ownedWeapons = new();
    private Dictionary<ItemCode, GameObject> instantiatedWeapons = new(); // dùng ItemCode làm key
    private bool wasWeaponActiveBeforeWallSlide = false; // Lưu trạng thái súng trước khi wall slide
    private CharacterController2D characterController;



    protected override void Start()
    {
        RefreshOwnedWeapons();
        ObserverManager.Instance.AddListener(EventID.EquipmentChanged, OnEquipmentChanged);


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
        if (GameStateManager.Instance.CurrentState == GameState.MiniGame)
        {
            this.TurnOffAllWeapon();
        }

        if (GameStateManager.Instance.CurrentState == GameState.Inventory) 
            return;

        if (characterController != null && characterController.isWallSliding)
            return;


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

        for (int i = 0; i < gunSlots.Length; i++)
        {
            var slot = gunSlots[i];
            if (slot != null && slot.HasItem())
            {
                var item = slot.currentItem;
                if (item.ItemProfileSO.weaponType == WeaponType.Gun)
                {
                    ownedWeapons.Add(item);
                }
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
        var key = item.ItemProfileSO.itemCode;

        if (weaponPrefab == null)
        {
            Debug.LogWarning("Vũ khí không có prefab.");
            return;
        }

        GameObject weaponInstance;

        if (instantiatedWeapons.TryGetValue(key, out var existing) && existing != null)
        {
            weaponInstance = existing;
        }
        else
        {
            weaponInstance = Instantiate(weaponPrefab, weaponHolder);
            weaponInstance.transform.localPosition = new Vector3(0.806f, 0f, 0f);
            weaponInstance.transform.localRotation = Quaternion.identity;
            instantiatedWeapons[key] = weaponInstance;
        }

        currentWeapon = weaponInstance;
        currentWeapon.SetActive(isWeaponActive);

        if (weaponHolder.TryGetComponent<WeaponAimer>(out var aimer))
        {
            aimer.SetCurrentWeapon(currentWeapon.transform);
        }
    }


    // Phương thức ẩn súng khi wall sliding
    public void HideWeaponDuringWallSlide()
    {
        isWeaponActive = false;
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
        //Debug.Log("Show ne");
        isWeaponActive = false;
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

    private void OnEquipmentChanged(object obj)
    {
        RefreshOwnedWeapons();

        // Lấy danh sách vũ khí hiện tại còn giữ lại
        HashSet<ItemCode> stillEquipped = new();
        foreach (var item in ownedWeapons)
        {
            if (item.ItemProfileSO != null)
                stillEquipped.Add(item.ItemProfileSO.itemCode);
        }

        // So sánh từng vũ khí đang được clone
        var keys = new List<ItemCode>(instantiatedWeapons.Keys);
        foreach (var key in keys)
        {
            if (!stillEquipped.Contains(key))
            {
                if (instantiatedWeapons[key] != null)
                    Destroy(instantiatedWeapons[key]);

                instantiatedWeapons.Remove(key);
            }
        }

        if (ownedWeapons.Count == 0)
        {
            TurnOffAllWeapon();

            if (currentWeapon != null)
            {
                Destroy(currentWeapon);
                currentWeapon = null;
            }
        }
    }
}