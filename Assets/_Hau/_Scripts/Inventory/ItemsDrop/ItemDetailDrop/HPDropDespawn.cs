using UnityEngine;

public class HPDropDespawn : ItemsDropDespawn
{
    private bool hasDespawned = false;

    public override void DoDespawn()
    {
        if (hasDespawned) return; // Ngăn gọi nhiều lần
        hasDespawned = true;
        base.DoDespawn();
    }


}
