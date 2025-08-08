using com.cyborgAssets.inspectorButtonPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InventoryTester : HauMonoBehaviour //E75 create
{
    public float currentIndex = 13;
    protected override void Start()
    {
        base.Start();
        //this.AddTestItems(ItemCode.Clothes_1, 1);
        //this.AddTestItems(ItemCode.Clothes_2, 1);
        //this.AddTestItems(ItemCode.Clothes_3, 1);
        //this.AddTestItems(ItemCode.Clothes_4, 1);

        //this.AddTestItems(ItemCode.Artefacts_1, 1);
        //this.AddTestItems(ItemCode.CraftingRecipe, 1);



        this.AddTestItems(ItemCode.MachineGun_0, 1);
        //this.AddTestItems(ItemCode.MachineGun_1, 1);
        //this.AddTestItems(ItemCode.MachineGun_3, 1);


        //this.AddTestItems(ItemCode.UpgradeItem_0, 3);
        //this.AddTestItems(ItemCode.UpgradeItem_1, 3);
        //this.AddTestItems(ItemCode.UpgradeItem_2, 3);
        //this.AddTestItems(ItemCode.UpgradeItem_3, 3);
        //this.AddTestItems(ItemCode.UpgradeItem_4, 3);
        //this.AddTestItems(ItemCode.UpgradeItem_5, 3);
        //this.AddTestItems(ItemCode.UpgradeItem_6, 3);



    }

    [ProButton]
    public virtual void AddTestItems(ItemCode itemCode, int count)
    {
        for (int i = 0; i < count; i++)
        {
            InventoryManager.Instance.AddItem(itemCode, 1);
        }
    }

    [ProButton]
    public virtual void RemoveTestItems(ItemCode itemCode, int count)
    {
        for (int i = 0; i < count; i++)
        {
            InventoryManager.Instance.RemoveItem(itemCode, 1);
        }
    }


    [ProButton]
    public virtual void PlaySound()
    {
        HauSoundManager.Instance.SpawnSound(Vector3.zero,SoundName.PickUpItem);
    }

    [ProButton]
    public virtual void LoadNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        SceneManager.LoadScene(nextSceneIndex);

    }
}
