using UnityEngine;

public class SpeechIconFollower : MonoBehaviour
{
    public Transform targetNPC;
    public Vector3 offset; // khoảng cách trên đầu

    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
    }

    void Update()
    {
        if (targetNPC != null)
        {
            Vector3 worldPos = targetNPC.position + offset;
            Vector3 screenPos = mainCam.WorldToScreenPoint(worldPos);
            transform.position = screenPos;
        }
    }
}
