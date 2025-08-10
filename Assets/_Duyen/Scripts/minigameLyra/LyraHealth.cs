using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LyraHealth : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;
    private int deathCount = 0;

    public GameObject gameOverPanel;
    public Image healthBarUI;
    public int CurrentHealth => currentHealth;

    public event Action OnDeath;

    private Transform respawnPoint;
    private float displayedHealth = 1f;

    public Transform healthBarTransform;

    private Material mat;
    private static readonly int HitBlendID = Shader.PropertyToID("_HitEffectBlend");
    private static readonly int HitColorID = Shader.PropertyToID("_HitColor");

    void Start()
    {
        // Tắt nếu không phải map điều khiển NPC
        if (FindAnyObjectByType<SceneController>() == null)
        {
            if (healthBarUI != null)
                healthBarUI.gameObject.SetActive(false);
            this.enabled = false;
            return;
        }

        // Bật thanh máu
        if (healthBarUI != null)
            healthBarUI.gameObject.SetActive(true);

        // Gán material từ SpriteRenderer
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            mat = sr.material;

        currentHealth = maxHealth;
        displayedHealth = 1f;

        TryFindRespawnPoint();

        // Chớp trắng lúc khởi tạo
        if (mat != null)
            StartCoroutine(FlashTwice(Color.white));
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

        // Chớp đỏ khi bị thương
        if (mat != null)
            StartCoroutine(FlashOnce(Color.red));

        if (currentHealth <= 0)
        {
            OnDeath?.Invoke();

            deathCount++;

            if (deathCount >= 3)
            {
                if (gameOverPanel != null)
                    gameOverPanel.SetActive(true);
                return;
            }

            // Hồi sinh
            currentHealth = maxHealth;
            if (respawnPoint != null)
                transform.position = respawnPoint.position;

            // Chớp trắng khi hồi sinh
            if (mat != null)
                StartCoroutine(FlashTwice(Color.white));
        }
    }

    void Update()
    {
        UpdateHealthBarSmooth();
        UpdateHealthBarDirection();
    }

    void UpdateHealthBarSmooth()
    {
        if (healthBarUI != null)
        {
            float targetFill = (float)currentHealth / maxHealth;
            displayedHealth = Mathf.Lerp(displayedHealth, targetFill, Time.deltaTime * 10f);
            healthBarUI.fillAmount = displayedHealth;
        }
    }

    void UpdateHealthBarDirection()
    {
        if (healthBarUI != null)
        {
            RectTransform rect = healthBarUI.GetComponent<RectTransform>();
            Vector3 scale = rect.localScale;
            scale.x = Mathf.Sign(transform.localScale.x) * Mathf.Abs(scale.x); // đảo hướng theo NPC
            rect.localScale = scale;
        }
    }

    void TryFindRespawnPoint()
    {
        GameObject found = GameObject.FindWithTag("spw");
        if (found != null)
        {
            respawnPoint = found.transform;
        }
    }

    IEnumerator FlashOnce(Color flashColor)
    {
        float duration = 0.1f;
        mat.SetColor(HitColorID, flashColor);
        mat.SetFloat(HitBlendID, 1f);
        yield return new WaitForSeconds(duration);
        mat.SetFloat(HitBlendID, 0f);
    }

    IEnumerator FlashTwice(Color flashColor)
    {
        float duration = 0.1f;
        for (int i = 0; i < 2; i++)
        {
            mat.SetColor(HitColorID, flashColor);
            mat.SetFloat(HitBlendID, .2f);
            yield return new WaitForSeconds(duration);
            mat.SetFloat(HitBlendID, 0f);
            yield return new WaitForSeconds(duration);
        }
    }

    public void ResetLyra()
    {
        deathCount = 0;
        currentHealth = maxHealth;

        if (respawnPoint != null)
            transform.position = respawnPoint.position;

        if (healthBarUI != null)
            healthBarUI.fillAmount = 1f;

        if (mat != null)
            StartCoroutine(FlashTwice(Color.white));
    }

}
