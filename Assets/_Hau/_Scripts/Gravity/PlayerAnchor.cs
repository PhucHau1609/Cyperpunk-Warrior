using UnityEngine;

public class PlayerAnchor : MonoBehaviour
{
    void LateUpdate()
    {
        // Luôn giữ Player đúng tại tâm của PlayerSwap
        transform.localPosition = Vector3.zero;
    }
}

