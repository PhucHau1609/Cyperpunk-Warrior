using System;
using System.Collections.Generic;
using UnityEngine;

public class PortalEnergyTracker : MonoBehaviour
{
    [SerializeField] private List<PortalReceiver> portals;
    [SerializeField] private GameObject doorObject; // Cửa cần mở
    [SerializeField] private string playerTag = "Player";

    private Animator doorAnimator;
    private Dictionary<PortalReceiver, EnergyType> portalStates = new();

    private bool allPortalsActivated = false;
    private bool playerInTrigger = false;
    private bool doorOpened = false;

    private void Awake()
    {
        // Lấy Animator từ đối tượng cửa
        if (doorObject != null)
        {
            doorAnimator = doorObject.GetComponent<Animator>();
        }

        foreach (var portal in portals)
        {
            portalStates[portal] = EnergyType.None; // ban đầu chưa có năng lượng
            portal.OnEnergySet += OnPortalEnergyChanged;
        }
    }

    private void OnDestroy()
    {
        foreach (var portal in portals)
        {
            portal.OnEnergySet -= OnPortalEnergyChanged;
        }
    }

    private void OnPortalEnergyChanged(PortalReceiver portal, EnergyType newEnergy)
    {
        portalStates[portal] = newEnergy;

        Debug.Log($"🌀 Portal {portal.name} now has energy: {newEnergy}");

        if (!allPortalsActivated && AllPortalsHaveEnergy())
        {
            allPortalsActivated = true;
            Debug.Log("✅ Tất cả các portal đã có năng lượng! Thực thi hành động...");
            // Gọi sự kiện hoặc trigger tùy ý ở đây
            OnAllPortalsEnergized();
        }
    }

    private bool AllPortalsHaveEnergy()
    {
        foreach (var energy in portalStates.Values)
        {
            if (energy == EnergyType.None)
                return false;
        }
        return true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInTrigger = true;
            Debug.Log("👣 Player đã vào vùng trigger.");
            OnAllPortalsEnergized();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInTrigger = false;
            Debug.Log("🚪 Player đã rời khỏi vùng trigger.");
        }
    }

    private void OnAllPortalsEnergized()
    {
        // TODO: Thêm logic bạn muốn tại đây
        //laze.SetActive(false);
        //Debug.Log("🔥 BẮT ĐẦU nhiệm vụ chính hoặc mở cổng chính!");
        
        //if (doorAnimator != null)
        //{
        //    doorAnimator.SetTrigger("open");
        //    Debug.Log("Đã bật trigger 'open' cho cửa!");
        //}
        //else
        //{
        //    Debug.LogWarning("Không tìm thấy Animator trên doorObject!");
        //}

        if (allPortalsActivated && playerInTrigger && !doorOpened)
        {
            doorOpened = true;
            if (doorAnimator != null)
            {
                doorAnimator.SetTrigger("open");
                Debug.Log("🚪 Cửa đã mở với animation 'open'!");
            }
            else
            {
                Debug.LogWarning("⚠️ Không tìm thấy Animator trên doorObject!");
            }
        }
    }
}
