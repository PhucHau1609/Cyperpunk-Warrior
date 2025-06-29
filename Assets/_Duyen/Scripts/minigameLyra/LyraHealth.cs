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
    private float displayedHealth = 1f; // Dùng cho hiệu ứng lerp mượt

    void Start()
    {
        // Nếu không phải map điều khiển NPC thì tắt script
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

        currentHealth = maxHealth;
        displayedHealth = 1f;

        TryFindRespawnPoint();
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0); // Không cho âm máu

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
            Debug.Log("Đã gán respawn point cho Lyra: " + respawnPoint.name);
        }
        else
        {
            Debug.LogWarning("Không tìm thấy GameObject có tag 'LyraRespawn' để làm respawn point!");
        }
    }
}
