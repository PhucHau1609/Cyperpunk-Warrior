using System;
using System.Collections.Generic;
using UnityEngine;

public class PortalEnergyTracker : MonoBehaviour
{
    [SerializeField] private List<PortalReceiver> portals;

    private Dictionary<PortalReceiver, EnergyType> portalStates = new();

    private bool allPortalsActivated = false;

    private void Awake()
    {
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

    private void OnAllPortalsEnergized()
    {
        // TODO: Thêm logic bạn muốn tại đây
        Debug.Log("🔥 BẮT ĐẦU nhiệm vụ chính hoặc mở cổng chính!");
    }
}
