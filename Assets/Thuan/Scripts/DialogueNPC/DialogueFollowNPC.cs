using UnityEngine;

public class DialogueFollowNPC : MonoBehaviour
{
    public Transform targetNPC;
    public Vector3 worldOffset; // Vị trí trên đầu NPC

    private Camera mainCam;
    private RectTransform rectTransform;

    void Start()
    {
        mainCam = Camera.main;
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (targetNPC != null)
        {
            Vector3 worldPos = targetNPC.position + worldOffset;
            Vector3 screenPos = mainCam.WorldToScreenPoint(worldPos);
            rectTransform.position = screenPos;
        }
    }
}
