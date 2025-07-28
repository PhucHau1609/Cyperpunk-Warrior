using UnityEngine;

public class ItemDropPoint : MonoBehaviour
{
    private void Start()
    {
        SpawnItemThisPosition();
    }

    public void SpawnItemThisPosition()
    {
        ItemsDropManager.Instance.DropItem(ItemCode.Artefacts_1, 1, this.transform.position);
    }    
}
