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

    [Header("Attribute Upgrade")]
    private CharacterController2D characterController;

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

    public static event System.Action<SkillID, float> OnEffectStarted;
    public static event System.Action<SkillID> OnEffectEnded;

    private Coroutine colorRampRoutine;  // giữ handle để hủy giữa chừng

    private void OnEnable()
    {
        // Lắng nghe thay đổi inventory/equipment
        ObserverManager.Instance?.AddListener(EventID.InventoryChanged, OnPrereqPossiblyChanged);
        ObserverManager.Instance?.AddListener(EventID.EquipmentChanged, OnPrereqPossiblyChanged);
    }

    private void OnDisable()
    {
        ObserverManager.Instance?.RemoveListener(EventID.InventoryChanged, OnPrereqPossiblyChanged);
        ObserverManager.Instance?.RemoveListener(EventID.EquipmentChanged, OnPrereqPossiblyChanged);
    }


    private void OnPrereqPossiblyChanged(object _ = null)
    {
        // Chỉ quan tâm skill ColorRamp và khi hiệu ứng đang active
        if (!isEffectActive) return;

        // Nếu điều kiện KHÔNG còn đủ -> tắt ngay
        bool stillMet = EquipmentConditionChecker.Instance != null &&
                        EquipmentConditionChecker.Instance.IsConditionMet();

        if (!stillMet)
        {
            // Hủy giữa chừng KHÔNG giữ cooldown (đúng yêu cầu: phải ấn lại nút sau khi đủ)
            ForceStopColorRamp(startCooldown: false);
            // Nếu bạn muốn vẫn tính cooldown khi bị hủy, đổi thành: ForceStopColorRamp(startCooldown: true);
        }
    }

    private void ForceStopColorRamp(bool startCooldown)
    {
        // Ngừng coroutine nếu còn đang chạy
        if (colorRampRoutine != null)
        {
            StopCoroutine(colorRampRoutine);
            colorRampRoutine = null;
        }

        // Tắt keyword + invincible
        string keyword = ShaderEffectKeywords[ShaderEffect.ColorRamp];
        SetKeywordOnSelf(keyword, false);
        if (characterController) characterController.invincible = false;

        // Reset trạng thái
        isEffectActive = false;

        // Thông báo UI khác tắt thanh duration, vòng effect...
        OnEffectEnded?.Invoke(SkillID.ColorRamp);

        if (startCooldown)
        {
            // vẫn tính cooldown phần còn lại
            StartCoroutine(CoCooldownOnly());
        }
        else
        {
            // không giữ cooldown -> có thể ấn lại nút nếu đủ điều kiện
            isOnCooldown = false;
        }
    }

    private IEnumerator CoCooldownOnly()
    {
        // nếu muốn tính phần còn lại, có thể tính theo thời gian đã trôi
        // Ở đây đơn giản: cooldown full
        yield return new WaitForSeconds(cooldownTime);
        isOnCooldown = false;
    }

    public bool ActivateColorRampEffectSkill()
    {
        // CHỈ dùng trạng thái hiện tại của trang bị + artefact
        bool prereq = EquipmentConditionChecker.Instance != null
                      && EquipmentConditionChecker.Instance.IsConditionMet();

        if (prereq && !isEffectActive && !isOnCooldown)
        {
            colorRampRoutine = StartCoroutine(ActivateColorRampEffect());
            OnEffectStarted?.Invoke(SkillID.ColorRamp, effectDuration);
            return true;
        }
        return false;
    }


    public void OnClickActivateColorRampIfReady()
    {
        bool condition = EquipmentConditionChecker.Instance != null &&
                         EquipmentConditionChecker.Instance.IsConditionMet();

        if (condition && !isEffectActive && !isOnCooldown)
        {
            ObserverManager.Instance.PostEvent(EventID.UnlockSkill_ColorRamp, SkillID.ColorRamp);
        }
    }

    IEnumerator ActivateColorRampEffect()
    {
        isEffectActive = true;
        isOnCooldown = true;

        string keyword = ShaderEffectKeywords[ShaderEffect.ColorRamp];
        SetKeywordOnSelf(keyword, true);

        if (characterController) characterController.invincible = true;

        // chạy effect
        yield return new WaitForSeconds(effectDuration);

        // Kết thúc effect bình thường
        SetKeywordOnSelf(keyword, false);
        if (characterController) characterController.invincible = false;

        isEffectActive = false;
        OnEffectEnded?.Invoke(SkillID.ColorRamp);

        float remainingCooldown = Mathf.Max(0f, cooldownTime - effectDuration);
        yield return new WaitForSeconds(remainingCooldown);

        isOnCooldown = false;
        colorRampRoutine = null;
    }





    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerShaderComponent = GetComponent<AllIn1Shader>();
        characterController = GetComponentInParent<CharacterController2D>();

        if (spriteRenderer != null)
        {
            spriteRenderer.material = new Material(spriteRenderer.material);
        }
    }

    public bool ActivateInvisibility()
    {
        if (!isEffectActive && !isOnCooldown &&
            PlayerStatus.Instance != null && PlayerStatus.Instance.UseEnergy(10f))
        {
            PlayerStatus.Instance.TriggerBlink(PlayerStatus.Instance.qImage);
            StartCoroutine(ActivateEffectWithInvisibility());
            OnEffectStarted?.Invoke(SkillID.Invisibility, effectDuration);
            return true;
        }
        return false;
    }

  /*  public bool ActivateColorRampEffectSkill()
    {
        if (ItemCollectionTracker.Instance.ConditionMet &&
            !isEffectActive && !isOnCooldown)
        {
            StartCoroutine(ActivateColorRampEffect());
            OnEffectStarted?.Invoke(SkillID.ColorRamp, effectDuration);
            return true;
        }
        return false;
    }

    public void OnClickActivateColorRampIfReady()
    {
        bool condition = EquipmentConditionChecker.Instance != null &&
                         EquipmentConditionChecker.Instance.IsConditionMet();

        if (condition && !isEffectActive && !isOnCooldown)
        {
            ObserverManager.Instance.PostEvent(EventID.UnlockSkill_ColorRamp, SkillID.ColorRamp);
        }
    }*/

    IEnumerator ActivateEffectWithInvisibility()
    {
        isEffectActive = true;
        isInvisible = true;
        isOnCooldown = true;

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

        isEffectActive = false;
        OnEffectEnded?.Invoke(SkillID.Invisibility);

        float remainingCooldown = Mathf.Max(0, cooldownTime - effectDuration);
        yield return new WaitForSeconds(remainingCooldown);

        isOnCooldown = false;
    }

    // PlayerShader.cs
   /* IEnumerator ActivateColorRampEffect()
    {
        isEffectActive = true;
        isOnCooldown = true;

        string keyword = ShaderEffectKeywords[ShaderEffect.ColorRamp];
        SetKeywordOnSelf(keyword, true);

        characterController.invincible = true;

        // chạy effect
        yield return new WaitForSeconds(effectDuration);

        SetKeywordOnSelf(keyword, false);
        characterController.invincible = false;
        isEffectActive = false;

        // cooldown phần còn lại để tổng = cooldownTime
        OnEffectEnded?.Invoke(SkillID.ColorRamp);

        float remainingCooldown = Mathf.Max(0f, cooldownTime - effectDuration); // << NEW
        yield return new WaitForSeconds(remainingCooldown);

        isOnCooldown = false;
    }*/


    /* IEnumerator ActivateColorRampEffect()
     {
         isEffectActive = true;
         isOnCooldown = true;
         //float originHp = characterController.life;


         string keyword = ShaderEffectKeywords[ShaderEffect.ColorRamp];
         SetKeywordOnSelf(keyword, true);

         characterController.invincible = true;
         //Debug.Log("🌈 Biến hình ColorRamp kích hoạt!");

         yield return new WaitForSeconds(effectDuration);

         SetKeywordOnSelf(keyword, false);
         //Debug.Log("🕒 Biến hình kết thúc. Bắt đầu hồi chiêu.");
         characterController.invincible = false;

         isEffectActive = false;

         yield return new WaitForSeconds(cooldownTime);
         isOnCooldown = false;

         //Debug.Log("✅ Hồi chiêu xong. Có thể biến hình lại.");
     }*/

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

