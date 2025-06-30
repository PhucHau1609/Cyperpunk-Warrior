using UnityEngine;
using System.Collections.Generic;

public class ObjectActivator : MonoBehaviour
{
    [Tooltip("Danh sách các đối tượng sẽ được kích hoạt khi Player bước vào vùng trigger")]
    public List<GameObject> targetObjects;

    [Tooltip("Tự động disable trigger sau khi kích hoạt")]
    public bool disableAfterTrigger = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            foreach (GameObject obj in targetObjects)
            {
                if (obj != null)
                    obj.SetActive(true);
            }

            if (disableAfterTrigger)
                gameObject.SetActive(false);
        }
    }
}
