using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerStatus : MonoBehaviour
{
    public Image hpBar;
    public Image energyBar;

    public float maxHP = 100f;
    public float currentHP = 100f;

    public float maxEnergy = 100f;
    public float currentEnergy = 100f;

    public Image qImage;
    public Image eImage;
    public Image rImage;

    void Awake()
    {
        // Nếu đã tồn tại 1 phiên bản PlayerStatus, thì huỷ phiên bản mới
        if (Object.FindObjectsByType<PlayerStatus>(FindObjectsSortMode.None).Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        StartCoroutine(RegenerateEnergy());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            TakeDamage(10f);
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (UseEnergy(10f))
                StartCoroutine(BlinkImage(qImage));
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (UseEnergy(10f))
                StartCoroutine(BlinkImage(eImage));
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (UseEnergy(10f))
                StartCoroutine(BlinkImage(rImage));
        }

        UpdateBars();
    }

    void TakeDamage(float amount)
    {
        currentHP = Mathf.Clamp(currentHP - amount, 0, maxHP);
    }

    bool UseEnergy(float amount)
    {
        if (currentEnergy >= amount)
        {
            currentEnergy -= amount;
            return true;
        }
        return false;
    }

    void UpdateBars()
    {
        if (hpBar != null)
            hpBar.fillAmount = currentHP / maxHP;

        if (energyBar != null)
            energyBar.fillAmount = currentEnergy / maxEnergy;
    }

    IEnumerator BlinkImage(Image img) //chớp chớp image 
    {
        if (img == null) yield break;

        Color originalColor = img.color;

        for (int i = 0; i < 3; i++)// Chớp 3 lần
        {
            img.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            img.color = originalColor;
            yield return new WaitForSeconds(0.1f);
        }
    }
    IEnumerator RegenerateEnergy() //hồi năng lượng 
    {
        while (true)
        {
            yield return new WaitForSeconds(2f);
            currentEnergy = Mathf.Clamp(currentEnergy + 1f, 0f, maxEnergy);
            UpdateBars();
        }
    }
}
