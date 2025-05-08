using com.cyborgAssets.inspectorButtonPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HauTesting : MonoBehaviour
{
    [ProButton]
    public void DropManyItemsTesting()
    {
        ItemsDropManager.Instance.DropManyItems(ItemCode.Gold, 1, new Vector3(0, 0, 0));
        ItemsDropManager.Instance.DropManyItems(ItemCode.EMP, 1, new Vector3(-1, 0, 0));
        ItemsDropManager.Instance.DropManyItems(ItemCode.Gun_1, 1, new Vector3(-3, 0, 0));
    }
}
