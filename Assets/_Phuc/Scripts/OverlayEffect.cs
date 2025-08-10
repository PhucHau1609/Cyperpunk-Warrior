using UnityEngine;

public class OverlayEffect : MonoBehaviour
{
    public SpriteRenderer overlayRenderer;
    [Range(0f, 1f)] public float opacity = 0.5f;

    void Update()
    {
        if (overlayRenderer != null)
        {
            var color = overlayRenderer.color;
            color.a = opacity;
            overlayRenderer.color = color;
        }
    }

    public void ChangeOverlay(Sprite newOverlay)
    {
        overlayRenderer.sprite = newOverlay;
    }
}
