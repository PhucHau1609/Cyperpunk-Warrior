using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class LightToggle : MonoBehaviour
{
    public Light2D light2D; // Gán thủ công hoặc tự động lấy

    public float interval = 2f; // Thời gian bật/tắt

    private void Start()
    {
        if (light2D == null)
            light2D = GetComponent<Light2D>();

        StartCoroutine(ToggleLight());
    }

    private IEnumerator ToggleLight()
    {
        while (true)
        {
            light2D.enabled = !light2D.enabled;
            yield return new WaitForSeconds(interval);
        }
    }
}
