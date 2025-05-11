using UnityEngine;
using UnityEngine.UI;

public class playerHealth : MonoBehaviour
{
    [Header("Máu Player")]
    public float health = 5f;
    public float maxHealth = 5f;

    [Header("UI Thanh Máu")]
    public Image healthBar;

    void Start()
    {
       
        if (maxHealth <= 0) maxHealth = health;
    }

    void Update()
    {
       
        healthBar.fillAmount = Mathf.Clamp01(health / maxHealth);
    }

    public void TakeDamage(float amount)
    {
        health -= amount;

        if (health <= 0)
        {
            health = 0;
            Debug.Log("Player chết!");
            
        }
    }
}
