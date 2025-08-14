using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using UnityEngine;

public class InventoryUIHandler : HauSingleton<InventoryUIHandler>
{
    [SerializeField] private GameObject inventoryIconUI;

    protected override void OnEnable()
    {
        ObserverManager.Instance?.AddListener(EventID.FirstItemPickedUp, OnFirstItemPickedUp);
    }

    protected override void OnDisable()
    {
        ObserverManager.Instance?.RemoveListener(EventID.FirstItemPickedUp, OnFirstItemPickedUp);
    }

    private void OnFirstItemPickedUp(object param)
    {
        inventoryIconUI.SetActive(true);
        //Debug.Log("🔔 Hiển thị icon Inventory vì nhặt vật phẩm đầu tiên.");
    }

    public void ToggleIconWhenPlayMiniGame()
    {
        inventoryIconUI.SetActive(!gameObject.activeSelf);
        //Debug.Log("clicked");
    }

    public void ShowIconInventory()
    {
        if (!InventoryManager.Instance.HasItemInInventory(ItemCode.CraftingRecipe)) return;
        inventoryIconUI.SetActive(true);

    }
}

/* private void Update()
   {
       if(GameStateManager.Instance.CurrentState == GameState.MiniGame)
       {
           inventoryIconUI.SetActive(false);
       }
       else if(GameStateManager.Instance.CurrentState == GameState.Gameplay)
       {
           inventoryIconUI.SetActive(true);

       }
   }*/

