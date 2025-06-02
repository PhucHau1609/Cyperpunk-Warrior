using AllIn1SpriteShader;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShader : MonoBehaviour
{
    [Header("Shader Settings")]
    public ShaderEffect effectToEnable = ShaderEffect.Hologram;
    public float effectDuration = 5f;

    private AllIn1Shader[] shaderTargets;
    private bool isEffectActive = false;

    private static readonly Dictionary<ShaderEffect, string> ShaderEffectKeywords = new Dictionary<ShaderEffect, string>
    {
        { ShaderEffect.Glow, "GLOW_ON" },
        { ShaderEffect.Fade, "FADE_ON" },
        { ShaderEffect.Outline, "OUTBASE_ON" },
        { ShaderEffect.AlphaOutline, "ALPHAOUTLINE_ON" },
        { ShaderEffect.InnerOutline, "INNEROUTLINE_ON" },
        { ShaderEffect.Gradient, "GRADIENT_ON" },
        { ShaderEffect.ColorSwap, "COLORSWAP_ON" },
        { ShaderEffect.HueShift, "HSV_ON" },
        { ShaderEffect.ChangeColor, "CHANGECOLOR_ON" },
        { ShaderEffect.ColorRamp, "COLORRAMP_ON" },
        { ShaderEffect.HitEffect, "HITEFFECT_ON" },
        { ShaderEffect.Negative, "NEGATIVE_ON" },
        { ShaderEffect.Pixelate, "PIXELATE_ON" },
        { ShaderEffect.GreyScale, "GREYSCALE_ON" },
        { ShaderEffect.Posterize, "POSTERIZE_ON" },
        { ShaderEffect.Blur, "BLUR_ON" },
        { ShaderEffect.MotionBlur, "MOTIONBLUR_ON" },
        { ShaderEffect.Ghost, "GHOST_ON" },
        { ShaderEffect.Hologram, "HOLOGRAM_ON" },
        { ShaderEffect.ChromaticAberration, "CHROMABERR_ON" },
        { ShaderEffect.Glitch, "GLITCH_ON" }
    };

    void Start()
    {
        shaderTargets = Object.FindObjectsByType<AllIn1Shader>(FindObjectsSortMode.None);

        if (shaderTargets.Length == 0)
        {
            Debug.LogWarning("Không tìm thấy bất kỳ AllIn1Shader nào trong scene.");
        }
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

        string keyword = ShaderEffectKeywords[effectToEnable];

        foreach (var shader in shaderTargets)
        {
            SetKeywordViaReflection(shader, keyword, true);
        }

        yield return new WaitForSeconds(effectDuration);

        foreach (var shader in shaderTargets)
        {
            SetKeywordViaReflection(shader, keyword, false);
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
