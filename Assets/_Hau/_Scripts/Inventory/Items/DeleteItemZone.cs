using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DeleteItemZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public static DeleteItemZone Instance;
    private bool isPointerOver = false;

    private void Awake()
    {
        Instance = this;
    }

    public void OnDrop(PointerEventData eventData)
    {
        // Có thể xử lý thêm ở đây nếu cần
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
    }

    public static bool IsPointerOverDeleteZone(PointerEventData eventData)
    {
        return Instance != null && Instance.isPointerOver;
    }
}