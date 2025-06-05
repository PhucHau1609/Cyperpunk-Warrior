using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerStatus : MonoBehaviour
{
    public Image energyBar;

    public float maxEnergy = 100f;
    public float currentEnergy = 100f;

    public Image qImage;
    public Image eImage;
    public Image rImage;

    void Awake()
    {
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
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (UseEnergy(10f))
                StartCoroutine(BlinkImage(qImage));
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            if (UseEnergy(10f))
                StartCoroutine(BlinkImage(eImage));
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (UseEnergy(10f))
                StartCoroutine(BlinkImage(rImage));
        }

        UpdateBars();
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
        if (energyBar != null)
            energyBar.fillAmount = currentEnergy / maxEnergy;
    }

    IEnumerator BlinkImage(Image img)
    {
        if (img == null) yield break;

        Color originalColor = img.color;

        for (int i = 0; i < 3; i++)
        {
            img.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            img.color = originalColor;
            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator RegenerateEnergy()
    {
        while (true)
        {
            yield return new WaitForSeconds(2f);
            currentEnergy = Mathf.Clamp(currentEnergy + 1f, 0f, maxEnergy);
            UpdateBars();
        }
    }
}
