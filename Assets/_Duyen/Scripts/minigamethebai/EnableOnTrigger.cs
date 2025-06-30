using UnityEngine;

public class EnableOnTrigger : MonoBehaviour
{
    [SerializeField] GameObject targetToEnable;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (targetToEnable != null)
                targetToEnable.SetActive(true);
        }
    }
}
