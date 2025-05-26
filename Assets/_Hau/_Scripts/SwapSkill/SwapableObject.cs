using Unity.Burst.CompilerServices;
using UnityEngine;

public class SwapableObject : MonoBehaviour
{
    public bool isSelected = false;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateVisual();
    }

    void OnMouseDown()
    {
        isSelected = !isSelected;
        UpdateVisual();
        SwapTargetManager.Instance.SetTarget(this, isSelected);
    }

    void UpdateVisual()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = isSelected ? Color.green : Color.white;
        }
    }
}
