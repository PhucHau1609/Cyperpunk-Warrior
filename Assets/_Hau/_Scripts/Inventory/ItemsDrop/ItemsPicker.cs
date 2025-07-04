using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using static UnityEditor.Progress;

[RequireComponent(typeof(CircleCollider2D))]
public class ItemsPicker : HauMonoBehaviour
{
    [SerializeField] protected CircleCollider2D sphere;
    [SerializeField] protected bool enableMousePicking = true;
    [SerializeField] protected float mousePickingRange = 3f;
    [SerializeField] protected LayerMask itemLayerMask = -1;

    protected Camera mainCamera;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        this.LoadSphereCollider();
        this.LoadCamera();
    }

    protected virtual void LoadSphereCollider()
    {
        if (this.sphere != null) return;
        this.sphere = GetComponent<CircleCollider2D>();
        this.sphere.radius = 0.6f;
        this.sphere.isTrigger = true;
        Debug.LogWarning(transform.name + ": LoadSphereCollider" + gameObject);
    }

    protected virtual void LoadCamera()
    {
        if (this.mainCamera != null) return;
        this.mainCamera = Camera.main;
        if (this.mainCamera == null)
            this.mainCamera = FindFirstObjectByType<Camera>();
    }

    protected virtual void Update()
    {
        if (enableMousePicking)
            this.HandleMousePicking();
    }

    protected virtual void HandleMousePicking()
    {
        // Kiểm tra click chuột trái
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPos = this.GetMouseWorldPosition();
            this.TryPickItemAtPosition(mouseWorldPos);
        }
    }

    protected virtual Vector3 GetMouseWorldPosition()
    {
        if (mainCamera == null) return Vector3.zero;

        Vector3 mousePos = Input.mousePosition;
        mousePos.z = mainCamera.nearClipPlane;
        return mainCamera.ScreenToWorldPoint(mousePos);
    }

    protected virtual void TryPickItemAtPosition(Vector3 worldPosition)
    {
        // Tìm item gần nhất trong phạm vi
        ItemsDropCtrl nearestItem = this.FindNearestItemInRange(worldPosition);

        if (nearestItem != null)
        {
            this.PickupItem(nearestItem);
        }
    }

    protected virtual ItemsDropCtrl FindNearestItemInRange(Vector3 position)
    {
        // Sử dụng OverlapCircle để tìm tất cả collider trong phạm vi
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, mousePickingRange, itemLayerMask);

        ItemsDropCtrl nearestItem = null;
        float nearestDistance = float.MaxValue;

        foreach (Collider2D collider in colliders)
        {
            if (collider.transform.parent == null) continue;

            ItemsDropCtrl itemCtrl = collider.transform.parent.GetComponent<ItemsDropCtrl>();
            if (itemCtrl == null) continue;

            // Kiểm tra khoảng cách từ player đến item
            float distanceToPlayer = Vector3.Distance(transform.position, itemCtrl.transform.position);
            if (distanceToPlayer > mousePickingRange) continue;

            // Tìm item gần nhất với vị trí click
            float distanceToClick = Vector3.Distance(position, itemCtrl.transform.position);
            if (distanceToClick < nearestDistance)
            {
                nearestDistance = distanceToClick;
                nearestItem = itemCtrl;
            }
        }

        return nearestItem;
    }

    // Hàm chung để pickup item (được sử dụng bởi cả trigger và mouse click)
    protected virtual void PickupItem(ItemsDropCtrl itemsDropCtrl)
    {
        if (itemsDropCtrl == null) return;

        ItemCollectionTracker.Instance.OnItemCollected(itemsDropCtrl.ItemCode);
        HauSoundManager.Instance.SpawnSound(Vector3.zero, SoundName.PickUpItem);
        itemsDropCtrl.Despawn.DoDespawn();
    }

    // Trigger pickup (giữ nguyên chức năng cũ)
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.transform.parent == null) return;
        ItemsDropCtrl itemsDropCtrl = other.transform.parent.GetComponent<ItemsDropCtrl>();
        if (itemsDropCtrl == null) return;

        this.PickupItem(itemsDropCtrl);
    }

   
}

/*[RequireComponent(typeof(CircleCollider2D))]
public class ItemsPicker : HauMonoBehaviour
{
    [SerializeField] protected CircleCollider2D sphere;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        this.LoadSphereCollider();
    }

    protected virtual void LoadSphereCollider()
    {
        if (this.sphere != null) return;
        this.sphere = GetComponent<CircleCollider2D>();
        this.sphere.radius = 0.6f;
        this.sphere.isTrigger = true;

        Debug.LogWarning(transform.name + ": LoadSphereCollider" + gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        this.TryPickItem(other);
    }

    public void TryPickItem(Collider2D other)
    {
        if (other == null || other.transform.parent == null) return;

        ItemsDropCtrl itemsDropCtrl = other.transform.parent.GetComponent<ItemsDropCtrl>();
        if (itemsDropCtrl == null) return;

        ItemCollectionTracker.Instance.OnItemCollected(itemsDropCtrl.ItemCode);
        HauSoundManager.Instance.SpawnSound(Vector3.zero, SoundName.PickUpItem);
        itemsDropCtrl.Despawn.DoDespawn();
    }
}*/
