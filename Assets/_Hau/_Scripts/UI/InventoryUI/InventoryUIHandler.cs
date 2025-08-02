using UnityEngine;

public class InventoryUIHandler : MonoBehaviour
{
    [SerializeField] private GameObject inventoryIconUI;

    private void OnEnable()
    {
        ObserverManager.Instance?.AddListener(EventID.FirstItemPickedUp, OnFirstItemPickedUp);
    }

    private void OnDisable()
    {
        ObserverManager.Instance?.RemoveListener(EventID.FirstItemPickedUp, OnFirstItemPickedUp);
    }

    private void OnFirstItemPickedUp(object param)
    {
        inventoryIconUI.SetActive(true);
        //Debug.Log("🔔 Hiển thị icon Inventory vì nhặt vật phẩm đầu tiên.");
    }
}

