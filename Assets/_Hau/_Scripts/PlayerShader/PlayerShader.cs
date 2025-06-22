using AllIn1SpriteShader;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerShader : MonoBehaviour
{
    [Header("Shader Settings")]
    public ShaderEffect effectToEnable = ShaderEffect.Hologram;
    public float effectDuration = 5f;
    public float cooldownTime = 20f;

    [Header("Invisibility")]
    public Light2D invisibilityLight;
    private SpriteRenderer spriteRenderer;
    private bool isInvisible = false;

    private AllIn1Shader playerShaderComponent;
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
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerShaderComponent = GetComponent<AllIn1Shader>();

        if (spriteRenderer != null)
        {
            // Nhân bản material để không dùng chung
            spriteRenderer.material = new Material(spriteRenderer.material);
        }

        if (playerShaderComponent == null)
        {
            Debug.LogWarning("Player chưa có component AllIn1Shader!");
        }
    }

    void Update()
    {
        // Nhấn J để bật shader + tàng hình trong effectDuration giây
        if (Input.GetKeyDown(KeyCode.J) && !isEffectActive)
        {
            StartCoroutine(ActivateEffectWithInvisibility());
        }

        // Nhấn phím 1 để biến hình ColorRamp (có cooldown)
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (ItemCollectionTracker.Instance.ConditionMet && !isEffectActive && !isOnCooldown)
            {
                StartCoroutine(ActivateColorRampEffect());
            }
        }
    }

    IEnumerator ActivateEffectWithInvisibility()
    {
        isEffectActive = true;
        isInvisible = true;

        string keyword = ShaderEffectKeywords[effectToEnable];
        SetKeywordOnSelf(keyword, true);

        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 0.149f;
            spriteRenderer.color = color;
        }

        if (invisibilityLight != null)
        {
            invisibilityLight.enabled = false;
        }

        Debug.Log("🔮 Shader & Tàng hình kích hoạt!");

        yield return new WaitForSeconds(effectDuration);

        SetKeywordOnSelf(keyword, false);

        isInvisible = false;

        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
        }

        if (invisibilityLight != null)
        {
            invisibilityLight.enabled = true;
        }

        Debug.Log("⏱️ Shader & Tàng hình kết thúc.");
        isEffectActive = false;
    }

    IEnumerator ActivateColorRampEffect()
    {
        isEffectActive = true;
        isOnCooldown = true;

        string keyword = ShaderEffectKeywords[ShaderEffect.ColorRamp];
        SetKeywordOnSelf(keyword, true);

        Debug.Log("🌈 Biến hình ColorRamp kích hoạt!");

        yield return new WaitForSeconds(effectDuration);

        SetKeywordOnSelf(keyword, false);

        Debug.Log("🕒 Biến hình kết thúc. Bắt đầu hồi chiêu.");
        isEffectActive = false;

        yield return new WaitForSeconds(cooldownTime);
        isOnCooldown = false;

        Debug.Log("✅ Hồi chiêu xong. Có thể biến hình lại.");
    }

    private void SetKeywordOnSelf(string keyword, bool state)
    {
        if (playerShaderComponent == null) return;
        playerShaderComponent.SetShaderKeyword(keyword, state);
    }
    public bool IsInvisible()
    {
        return isInvisible;
    }
}


/*  private void SetKeywordOnSelf(string keyword, bool state)
  {
      if (playerShaderComponent == null) return;

      playerShaderComponent.SendMessage("SetSceneDirty", SendMessageOptions.DontRequireReceiver);
      playerShaderComponent.GetType()
          .GetMethod("SetKeyword", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
          ?.Invoke(playerShaderComponent, new object[] { keyword, state });
  }*/

