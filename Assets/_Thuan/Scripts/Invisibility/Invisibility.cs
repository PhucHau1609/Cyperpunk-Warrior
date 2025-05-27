using UnityEngine;

public class Invisibility : MonoBehaviour
{
    public Material invisibleMaterial;
    public GameObject npcLyra; // Gán NPC (Lyra) từ Inspector
    private InvisibilityNPC npcScript;

    private Material defaultMaterial;
    private SpriteRenderer spriteRenderer;
    private bool isInvisible = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        defaultMaterial = spriteRenderer.sharedMaterial;

        // Lấy Script từ NPC
        if (npcLyra != null)
        {
            npcScript = npcLyra.GetComponent<InvisibilityNPC>();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J) && !isInvisible)
        {
            StartCoroutine(ActivateInvisibility());

            // Gọi NPC tàng hình theo
            if (npcScript != null)
            {
                npcScript.TriggerInvisibility();
            }
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
