using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PickableByClick : MonoBehaviour
{
    protected virtual void OnMouseDown()
    {
        Debug.Log("Item click by mouse");
        ItemsPicker picker = FindFirstObjectByType<ItemsPicker>();
        if (picker == null) return;

        Collider2D col = GetComponent<Collider2D>();
       //picker.TryPickItem(col);
    }
}
