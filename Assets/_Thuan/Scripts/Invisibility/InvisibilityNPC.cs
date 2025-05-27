using UnityEngine;

public class InvisibilityNPC : MonoBehaviour
{
    public Material invisibleMaterial;

    private Material defaultMaterial;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        defaultMaterial = spriteRenderer.sharedMaterial;
    }

    public void TriggerInvisibility()
    {
        StopAllCoroutines(); // Dừng các coroutine cũ (nếu có)
        StartCoroutine(ActivateInvisibility());
    }

    System.Collections.IEnumerator ActivateInvisibility()
    {
        spriteRenderer.sharedMaterial = invisibleMaterial;

        yield return new WaitForSeconds(5f);

        spriteRenderer.sharedMaterial = defaultMaterial;
    }
}
