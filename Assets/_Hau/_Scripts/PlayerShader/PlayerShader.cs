using AllIn1SpriteShader;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShader : MonoBehaviour
{
    [Header("Shader Settings")]
    public ShaderEffect effectToEnable = ShaderEffect.Hologram;
    public float effectDuration = 5f;
    public float cooldownTime = 20f;


    private AllIn1Shader[] shaderTargets;
    private bool isEffectActive = false;
    private bool isOnCooldown = false;


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
        // Giữ phím J để test shader thông thường
        if (Input.GetKeyDown(KeyCode.J) && !isEffectActive)
        {
            StartCoroutine(ActivateEffect());
        }

        // Nhấn phím [1] để kích hoạt hiệu ứng biến hình (ColorRamp)
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (ItemCollectionTracker.Instance.ConditionMet && !isEffectActive && !isOnCooldown)
            {
                StartCoroutine(ActivateColorRampEffect());
            }
        }
    }

    // Dành riêng cho test/phím J
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

    // Dành riêng cho biến hình bằng phím 1 (ColorRamp có cooldown)
    IEnumerator ActivateColorRampEffect()
    {
        isEffectActive = true;
        isOnCooldown = true;

        string keyword = ShaderEffectKeywords[ShaderEffect.ColorRamp];

        foreach (var shader in shaderTargets)
        {
            SetKeywordViaReflection(shader, keyword, true);
        }

        Debug.Log("🌈 Biến hình ColorRamp kích hoạt!");

        yield return new WaitForSeconds(effectDuration);

        foreach (var shader in shaderTargets)
        {
            SetKeywordViaReflection(shader, keyword, false);
        }

        Debug.Log("🕒 Biến hình kết thúc. Bắt đầu hồi chiêu.");

        isEffectActive = false;

        yield return new WaitForSeconds(cooldownTime);
        isOnCooldown = false;

        Debug.Log("✅ Hồi chiêu xong. Có thể biến hình lại.");
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
