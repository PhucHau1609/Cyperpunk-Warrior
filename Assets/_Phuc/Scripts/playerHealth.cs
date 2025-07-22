using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class playerHealth : MonoBehaviour
{
    [Header("UI Thanh Máu")]
    [SerializeField] private Image healthBar;

    [Header("Hiệu ứng hồi máu")]
    public GameObject healEffect;

    [Header("Hiệu ứng Damage Overlay")]
    [SerializeField] private Image damageOverlay; // Panel đỏ
    [SerializeField] private float overlayDuration = 0.5f;
    [SerializeField] private Color flashColor = new Color(1f, 0f, 0f, 0.5f);

    private CharacterController2D characterController;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        characterController = GetComponent<CharacterController2D>();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Update()
    {
        if (characterController != null && healthBar != null)
        {
            float current = characterController.life;
            float max = characterController.maxLife;

            healthBar.fillAmount = Mathf.Clamp01(current / max);
        }
    }

    public void TakeDamage(float amount)
    {
        if (characterController != null)
        {
            characterController.ApplyDamage(amount, this.transform.position);
            ShowDamageOverlay(); // Gọi hiệu ứng đỏ khi trúng đòn
        }
    }

    public void Heal(float amount)
    {
        if (characterController != null)
        {
            characterController.life = Mathf.Clamp(
                characterController.life + amount,
                0f,
                characterController.maxLife
            );
            PlayHealEffect();
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject found = GameObject.FindWithTag("HealthBar");
        if (found != null)
        {
            healthBar = found.GetComponent<Image>();
        }
    }

    public void PlayHealEffect()
    {
        if (healEffect == null) return;

        StopAllCoroutines();
        StartCoroutine(HealEffectRoutine());
    }

    IEnumerator HealEffectRoutine()
    {
        healEffect.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        healEffect.SetActive(false);
    }

    public void ShowDamageOverlay()
    {
        if (characterController.invincible) return;
        if (damageOverlay == null) return;

        StopCoroutine(nameof(DamageOverlayRoutine));
        StartCoroutine(DamageOverlayRoutine());
    }

    IEnumerator DamageOverlayRoutine()
    {
        damageOverlay.color = flashColor;

        float elapsed = 0f;
        while (elapsed < overlayDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(flashColor.a, 0f, elapsed / overlayDuration);
            damageOverlay.color = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);
            yield return null;
        }

        damageOverlay.color = new Color(0, 0, 0, 0); // ẩn hoàn toàn
    }
}
