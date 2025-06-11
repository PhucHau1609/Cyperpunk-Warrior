using com.cyborgAssets.inspectorButtonPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HauTesting : MonoBehaviour
{
    [ProButton]
    public void DropManyItemsTesting()
    {
       /* ItemsDropManager.Instance.DropManyItems(ItemCode.Clothes_1, 1, new Vector3(0, 0, 0));
        ItemsDropManager.Instance.DropManyItems(ItemCode.Clothes_2, 1, new Vector3(0, 0, 0));
        ItemsDropManager.Instance.DropManyItems(ItemCode.Clothes_3, 1, new Vector3(0, 0, 0));
        ItemsDropManager.Instance.DropManyItems(ItemCode.Clothes_4, 1, new Vector3(0, 0, 0));
        ItemsDropManager.Instance.DropManyItems(ItemCode.Artefacts_1, 1, new Vector3(-5, -2, 0));*/
        ItemsDropManager.Instance.DropManyItems(ItemCode.MachineGun_0, 1, new Vector3(-5, -2, 0));


    }

    [ProButton]
    private void ShowHitEffect()
    {

        Vector3 hitPos = Vector3.zero;
        EffectCtrl prefab = EffectSpawnerCtrl.Instance.EffectSpawner.PoolPrefabs.GetPrefabByName("Fire_1");
        EffectCtrl newHitEffect = EffectSpawnerCtrl.Instance.EffectSpawner.Spawn(prefab, hitPos);
        newHitEffect.gameObject.SetActive(true);
    }
}
