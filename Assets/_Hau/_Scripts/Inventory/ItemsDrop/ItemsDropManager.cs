using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class ItemsDropManager : HauSingleton<ItemsDropManager>
{
    [SerializeField] protected ItemsDropSpawner itemsDropSpawner;
    public ItemsDropSpawner ItemsDropSpawner => itemsDropSpawner;

    protected float spawnHeight = 1.0f;
    protected float forceAmount = 5.0f;

    protected override void Awake()
    {
        base.Awake();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        this.ClearAllDroppedItems();
    }

    public void ClearAllDroppedItems()
    {
        //Debug.Log("🧹 ClearAllDroppedItems");

        // 1. Clear all active items from pool holder (chính xác nhất)
        if (itemsDropSpawner.PoolHolder is Transform poolHolder)
        {
            foreach (Transform child in poolHolder)
            {
                ItemsDropCtrl item = child.GetComponent<ItemsDropCtrl>();
                if (item != null && item.gameObject.activeSelf)
                {
                    itemsDropSpawner.Despawn(item);
                    Debug.Log("🟡 Despawned: " + item.name);
                }
            }
        }

        // 2. Optional: clear inPoolObj nếu muốn
        itemsDropSpawner.InPoolObj.Clear();
    }



    protected override void LoadComponents()
    {
        base.LoadComponents();
        this.LoadItemsDropSpawner();
    }

    protected virtual void LoadItemsDropSpawner()
    {
        if (this.itemsDropSpawner != null) return;
        this.itemsDropSpawner = GetComponent<ItemsDropSpawner>();

        Debug.LogWarning(transform.name + ": LoadItemsDropSpawner" + gameObject);
    }

    public virtual void DropManyItems(ItemCode itemCode, int dropCount, Vector3 dropPosition)
    {
        for (int i = 0; i < dropCount; i++)
        {
            this.DropItem(itemCode, 1, dropPosition);
        }
    }

    public virtual void DropItem(ItemCode itemCode, int dropCount, Vector3 dropPosition)
    {
        Vector3 spawnPosition = dropPosition + new Vector3(0, spawnHeight, 0);
        ItemsDropCtrl itemPrefab = this.itemsDropSpawner.PoolPrefabs.GetPrefabByName(itemCode.ToString());
        if (itemPrefab == null) itemPrefab = this.itemsDropSpawner.PoolPrefabs.GetPrefabByName("DefaultDrop");

        ItemsDropCtrl newItem = this.itemsDropSpawner.Spawn(itemPrefab, spawnPosition);
        newItem.SetValueItemDrop(itemCode, dropCount);
        newItem.gameObject.SetActive(true);

        Vector3 randomDirection = Random.onUnitSphere;
        randomDirection.y = Mathf.Abs(randomDirection.y);
        newItem.Rigidbody.AddForce(randomDirection * forceAmount, ForceMode2D.Impulse);
    }

    public virtual ItemsDropCtrl DropItemObject(ItemCode itemCode, int dropCount, Vector3 dropPosition)
    {
        Vector3 spawnPosition = dropPosition + new Vector3(0, spawnHeight, 0);
        ItemsDropCtrl itemPrefab = this.itemsDropSpawner.PoolPrefabs.GetPrefabByName(itemCode.ToString());
        if (itemPrefab == null) itemPrefab = this.itemsDropSpawner.PoolPrefabs.GetPrefabByName("DefaultDrop");

        ItemsDropCtrl newItem = this.itemsDropSpawner.Spawn(itemPrefab, spawnPosition);
        newItem.SetValueItemDrop(itemCode, dropCount);
        newItem.gameObject.SetActive(true);

        Vector3 randomDirection = Random.onUnitSphere;
        randomDirection.y = Mathf.Abs(randomDirection.y);
        newItem.Rigidbody.AddForce(randomDirection * forceAmount, ForceMode2D.Impulse);

        return newItem;
    }

}
