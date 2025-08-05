using com.cyborgAssets.inspectorButtonPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HauTesting : MonoBehaviour
{

    private void Start()
    {
        //ItemsDropManager.Instance.DropManyItems(ItemCode.MachineGun_1, 1, new Vector3(0, 0, 0));
        //ItemsDropManager.Instance.DropManyItems(ItemCode.MachineGun_0, 1, new Vector3(0, 0, 0));

    }
    /* private void Update()
     {
         if(Input.GetKeyDown(KeyCode.R))
         {
             this.DropManyItemsTesting();
         }    
     }*/

    [ProButton]
    public void DropManyItemsTesting()
    {
        //ItemsDropManager.Instance.DropManyItems(ItemCode.CraftingRecipe, 1, new Vector3(0, -1, 0));
        ItemsDropManager.Instance.DropManyItems(ItemCode.Clothes_1, 1, new Vector3(-1, -1, 0));
        ItemsDropManager.Instance.DropManyItems(ItemCode.Clothes_2, 1, new Vector3(0, 0, 0));
        ItemsDropManager.Instance.DropManyItems(ItemCode.Clothes_3, 1, new Vector3(0, 0, 0));
        ItemsDropManager.Instance.DropManyItems(ItemCode.Clothes_4, 1, new Vector3(0, 0, 0));
        ItemsDropManager.Instance.DropManyItems(ItemCode.Clothes_5, 1, new Vector3(0, 0, 0));
        ItemsDropManager.Instance.DropManyItems(ItemCode.Artefacts_1, 1, new Vector3(-4, -2, 0));

        //ItemsDropManager.Instance.DropManyItems(ItemCode.MachineGun_0, 1, new Vector3(30, 5, 0));
        //ItemsDropManager.Instance.DropManyItems(ItemCode.MachineGun_1, 1, new Vector3(30, 5, 0));
        //ItemsDropManager.Instance.DropManyItems(ItemCode.MachineGun_3, 1, new Vector3(30, 5, 0));

        //ItemsDropManager.Instance.DropManyItems(ItemCode.HP, 1, new Vector3(5, -1, 0));

        //ItemsDropManager.Instance.DropManyItems(ItemCode.UpgradeItem_0, 1, new Vector3(0, 0, 0));
        //ItemsDropManager.Instance.DropManyItems(ItemCode.UpgradeItem_1, 1, new Vector3(1, 0, 0));
        //ItemsDropManager.Instance.DropManyItems(ItemCode.UpgradeItem_2, 1, new Vector3(2, 0, 0));



    }

    [ProButton]
    private void ShowHitEffect()
    {

        Vector3 hitPos = Vector3.zero;
        EffectCtrl prefab = EffectSpawnerCtrl.Instance.EffectSpawner.PoolPrefabs.GetPrefabByName("Fire_1");
        EffectCtrl newHitEffect = EffectSpawnerCtrl.Instance.EffectSpawner.Spawn(prefab, hitPos);
        newHitEffect.gameObject.SetActive(true);
    }
    [ProButton]
    public virtual void AddItem(ItemCode itemCode, int itemCount)
    {
        ItemProfileSO itemProfile = InventoryManager.Instance.GetProfileByCode(itemCode);
        ItemInventory item = new(itemProfile, itemCount);
        InventoryManager.Instance.AddItem(item);
    }
}
