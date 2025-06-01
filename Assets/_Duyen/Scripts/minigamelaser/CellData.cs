using UnityEngine;
using UnityEngine.UI;

public class CellData : MonoBehaviour
{
    public Vector2Int gridPosition;
    public GameObject currentObject; // Gương, tường, emitter, target...

    public void SetObject(GameObject obj)
    {
        if (currentObject != null) Destroy(currentObject);
        currentObject = Instantiate(obj, transform);
    }
}
