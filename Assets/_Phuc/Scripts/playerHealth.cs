using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class playerHealth : MonoBehaviour
{
    [Header("UI Thanh Máu")]
    public Image healthBar;

    [Header("Hiệu ứng hồi máu")]
    public GameObject healEffect;

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
        }
    }

    public void Heal(float amount)
    {
        if (characterController != null)
        {
            characterController.life = Mathf.Clamp(
                characterController.life + amount,   // cộng máu
                0f,                                  // không dưới 0
                characterController.maxLife          // không vượt quá max thực tế của player
            );
            PlayHealEffect(); // chạy hiệu ứng hồi máu (nếu có)
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

    System.Collections.IEnumerator HealEffectRoutine()
    {
        healEffect.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        healEffect.SetActive(false);
    }

}
