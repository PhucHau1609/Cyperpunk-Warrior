using UnityEngine;

public class Invisibility : MonoBehaviour
{
    public Material invisibleMaterial;
    private Material defaultMaterial;
    private SpriteRenderer spriteRenderer;
    private bool isInvisible = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        defaultMaterial = spriteRenderer.sharedMaterial;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J) && !isInvisible)
        {
            StartCoroutine(ActivateInvisibility());
        }
    }

    System.Collections.IEnumerator ActivateInvisibility()
    {
        isInvisible = true;

        spriteRenderer.sharedMaterial = invisibleMaterial;

        yield return new WaitForSeconds(5f);

        spriteRenderer.sharedMaterial = defaultMaterial;

        isInvisible = false;
    }
}
