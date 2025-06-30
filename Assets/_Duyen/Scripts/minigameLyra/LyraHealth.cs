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

    private Transform respawnPoint;
    private float displayedHealth = 1f;

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

    void TryFindRespawnPoint()
    {
        GameObject found = GameObject.FindWithTag("LyraRespawn");
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
}
