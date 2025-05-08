using TMPro;
using UnityEngine;

public class BlinkText : MonoBehaviour
{
    public float blinkSpeed = 0.5f;
    private TextMeshProUGUI textMesh;
    private bool isVisible = true;
    private float timer;

    void Start()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        timer = blinkSpeed;
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            isVisible = !isVisible;
            textMesh.alpha = isVisible ? 0.4f : 0f;
            timer = blinkSpeed;
        }
    }
}
