using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class Explosion : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;
    private AudioSource audioSource;
    private Light2D spotLight;
    
    public AudioClip explosionSound;
    
    // Cài đặt cho hiệu ứng ánh sáng
    [Header("Light Effect Settings")]
    public float lightGrowDuration = 1f; // Thời gian tăng dần ánh sáng
    public float lightHoldDuration = 0.5f; // Thời gian dừng lại ở max
    public float lightFadeDuration = 0.8f; // Thời gian giảm dần ánh sáng
    public float targetIntensity = 1000f;
    public float targetOuterRadius = 32f;
    
    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        
        // Tìm Spot Light 2D trong GameObject con (có thể null nếu không có)
        spotLight = GetComponentInChildren<Light2D>();
        
        // Hoặc nếu bạn biết tên của GameObject con:
        // spotLight = transform.Find("TenGameObjectCon")?.GetComponent<Light2D>();
    }
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground"))
        {
            rb.bodyType = RigidbodyType2D.Static;
            anim.SetTrigger("Explode");
            audioSource.PlayOneShot(explosionSound);
            
            // Chỉ điều khiển Spot Light 2D nếu nó tồn tại
            if (spotLight != null)
            {
                Debug.Log("Explosion light effect activated!");
                
                // Bắt đầu hiệu ứng ánh sáng tăng dần
                StartCoroutine(AnimateLight());
                
                // Destroy sau khi hoàn thành cả 3 giai đoạn
                float totalLightDuration = lightGrowDuration + lightHoldDuration + lightFadeDuration;
                Destroy(gameObject, totalLightDuration + 0.5f);
            }
            else
            {
                Debug.Log("No Spot Light found - explosion without light effect");
                
                // Destroy nhanh hơn khi không có Spot Light
                Destroy(gameObject, 1.5f);
            }
        }
    }
    
    IEnumerator AnimateLight()
    {
        // Lưu giá trị ban đầu
        float startIntensity = spotLight.intensity;
        float startRadius = spotLight.pointLightOuterRadius;
        
        // Tối ưu chất lượng ánh sáng để tránh viền đen
        if (spotLight.lightType == Light2D.LightType.Point)
        {
            // Cải thiện chất lượng ánh sáng
            spotLight.pointLightInnerRadius = 0.1f; // Inner radius nhỏ để tránh viền cứng
            spotLight.pointLightOuterAngle = 360f; // Đảm bảo ánh sáng tròn đầy đủ
            spotLight.falloffIntensity = 0.5f; // Giảm falloff để ánh sáng mượt hơn
        }
        
        // Phase 1: Tăng dần từ giá trị ban đầu lên max
        yield return StartCoroutine(LightGrowPhase(startIntensity, startRadius));
        
        // Phase 2: Dừng lại ở max trong 1 khoảng thời gian
        yield return StartCoroutine(LightHoldPhase());
        
        // Phase 3: Giảm dần từ max xuống 1
        yield return StartCoroutine(LightFadePhase());
    }
    
    IEnumerator LightGrowPhase(float startIntensity, float startRadius)
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < lightGrowDuration)
        {
            float progress = elapsedTime / lightGrowDuration;
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);
            
            // Tăng dần intensity và radius lên max
            spotLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, smoothProgress);
            spotLight.pointLightOuterRadius = Mathf.Lerp(startRadius, targetOuterRadius, smoothProgress);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Đảm bảo giá trị max chính xác
        spotLight.intensity = targetIntensity;
        spotLight.pointLightOuterRadius = targetOuterRadius;
    }
    
    IEnumerator LightHoldPhase()
    {
        // Dừng lại ở giá trị max trong khoảng thời gian nhất định
        yield return new WaitForSeconds(lightHoldDuration);
        
        // Trong thời gian này, ánh sáng giữ nguyên:
        // intensity = targetIntensity (1000)
        // pointLightOuterRadius = targetOuterRadius (32)
    }
    
    IEnumerator LightFadePhase()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < lightFadeDuration)
        {
            float progress = elapsedTime / lightFadeDuration;
            
            // Các tùy chọn tốc độ giảm (chọn 1 trong các dòng dưới):
            
            // float fastProgress = progress;                           // Tuyến tính (đều)
            // float fastProgress = Mathf.Pow(progress, 0.3f);          // Rất nhanh ở đầu
            float fastProgress = Mathf.Pow(progress, 0.5f);             // Nhanh ở đầu (hiện tại)
            // float fastProgress = Mathf.Pow(progress, 1.5f);          // Chậm ở đầu, nhanh ở cuối
            // float fastProgress = progress * progress;                // Chậm ở đầu, rất nhanh ở cuối
            // float fastProgress = Mathf.SmoothStep(0f, 1f, progress); // Mượt nhất
            
            // Giảm dần intensity và radius xuống 1
            spotLight.intensity = Mathf.Lerp(targetIntensity, 1f, fastProgress);
            spotLight.pointLightOuterRadius = Mathf.Lerp(targetOuterRadius, 1f, fastProgress);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Đảm bảo giá trị cuối chính xác
        spotLight.intensity = 1f;
        spotLight.pointLightOuterRadius = 1f;
    }
}