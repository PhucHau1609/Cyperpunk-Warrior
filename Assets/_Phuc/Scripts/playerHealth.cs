using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class playerHealth : MonoBehaviour
{
    [Header("Máu Player")]
    public float health = 5f;
    public float maxHealth = 5f;

    [Header("UI Thanh Máu")]
    public Image healthBar;

    [Header("Hiệu ứng hồi máu")]
    public GameObject healEffect; // ← gán prefab hoặc GameObject con tại đây

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

    void Start()
    {
        if (maxHealth <= 0) maxHealth = health;

        if (healEffect != null)
            healEffect.SetActive(false); // Tắt hiệu ứng lúc đầu
    }

    void Update()
    {
        if (healthBar != null)
            healthBar.fillAmount = Mathf.Clamp01(health / maxHealth);
    }

    public void TakeDamage(float amount)
    {
        health -= amount;

        
        characterController.ApplyDamage(amount,this.transform.position);

        if (health <= 0)
        {
            health = 0;
            //Debug.Log("Player chết!");
        }
    }

    public void Heal(float amount)
    {
        health = Mathf.Clamp(health + amount, 0f, maxHealth);
       
        PlayHealEffect(); 
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

    System.Collections.IEnumerator HealEffectRoutine()
    {
        healEffect.SetActive(true);
        yield return new WaitForSeconds(1.5f); 
        healEffect.SetActive(false);
    }
}
