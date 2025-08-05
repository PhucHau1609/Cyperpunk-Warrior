using System.Collections;
using UnityEngine;
using AllIn1SpriteShader;

public class TriggerShaderEffect : MonoBehaviour
{
    [Header("Shader Settings")]
    public string keywordToEnable = "HOLOGRAM_ON";
    public float effectDuration = 5f;

    private AllIn1Shader[] shaderTargets;
    private bool isEffectActive = false;

    void Start()
    {
        // Tự tìm các đối tượng có AllIn1Shader trong scene (bao gồm cả DontDestroyOnLoad)
        AllIn1Shader[] allShaders = Object.FindObjectsByType<AllIn1Shader>(FindObjectsSortMode.None);
        shaderTargets = allShaders;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J) && !isEffectActive)
        {
            StartCoroutine(ActivateEffect());
        }
    }

    IEnumerator ActivateEffect()
    {
        isEffectActive = true;

        foreach (var shader in shaderTargets)
        {
            SetKeywordViaReflection(shader, keywordToEnable, true);
        }

        yield return new WaitForSeconds(effectDuration);

        foreach (var shader in shaderTargets)
        {
            SetKeywordViaReflection(shader, keywordToEnable, false);
        }

        isEffectActive = false;
    }

    private void SetKeywordViaReflection(AllIn1Shader shader, string keyword, bool state)
    {
        if (shader == null) return;

        shader.SendMessage("SetSceneDirty", SendMessageOptions.DontRequireReceiver);
        shader.GetType()
              .GetMethod("SetKeyword", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
              ?.Invoke(shader, new object[] { keyword, state });
    }
}
