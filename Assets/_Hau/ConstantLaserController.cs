using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(EdgeCollider2D), typeof(AudioSource))]
public class ConstantLaserController : MonoBehaviour
{
    [Header("Laser Settings")]
    public float maxLaserDistance = 20f;

    [Header("References")]
    public LineRenderer laserRenderer;
    public Transform firePoint;
    public LayerMask groundLayer;
    public GameObject hitEffectPrefab;

    [Header("Damage Settings")]
    public float damage = 10f;

    private GameObject currentHitEffect;
    private EdgeCollider2D edgeCollider;
    private AudioSource laserAudio;

    private void Start()
    {
        laserRenderer.enabled = true;

        edgeCollider = GetComponent<EdgeCollider2D>();
        edgeCollider.isTrigger = true;
        edgeCollider.enabled = true;

        laserAudio = GetComponent<AudioSource>();
        laserAudio.playOnAwake = false;

        // Phát âm thanh laser khi bắt đầu (nếu cần lặp lại, hãy bật loop trong AudioSource)
        if (laserAudio != null) laserAudio.Play();
    }

    private void Update()
    {
        UpdateLaser();
    }

    void UpdateLaser()
    {
        RaycastHit2D hit = Physics2D.Raycast(firePoint.position, firePoint.right, maxLaserDistance, groundLayer);

        if (hit.collider != null && hit.collider.CompareTag("Ground"))
        {
            DrawLaser(firePoint.position, hit.point);
            ShowHitEffect(hit.point);
        }
        else
        {
            Vector2 endPos = firePoint.position + firePoint.right * maxLaserDistance;
            DrawLaser(firePoint.position, endPos);
            HideHitEffect();
        }
    }

    void DrawLaser(Vector2 start, Vector2 end)
    {
        laserRenderer.SetPosition(0, start);
        laserRenderer.SetPosition(1, end);

        Vector2[] points = new Vector2[2];
        points[0] = transform.InverseTransformPoint(start);
        points[1] = transform.InverseTransformPoint(end);
        edgeCollider.SetPoints(new List<Vector2>(points));
    }

    void ShowHitEffect(Vector2 position)
    {
        if (hitEffectPrefab == null) return;

        if (currentHitEffect == null)
        {
            currentHitEffect = Instantiate(hitEffectPrefab, position, Quaternion.identity);
        }
        else
        {
            currentHitEffect.transform.position = position;
            currentHitEffect.SetActive(true);
        }
    }

    void HideHitEffect()
    {
        if (currentHitEffect != null)
        {
            currentHitEffect.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerHealth pHealth = other.GetComponent<playerHealth>();
            if (pHealth != null)
            {
                pHealth.TakeDamage(damage);
            }
        }
    }
}
