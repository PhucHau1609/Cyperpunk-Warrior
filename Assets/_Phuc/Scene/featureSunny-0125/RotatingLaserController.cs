using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(EdgeCollider2D), typeof(AudioSource))]
public class RotatingLaserController : MonoBehaviour
{
    [Header("Laser Settings")]
    public float laserActiveDuration = 2f;
    public float laserCooldownDuration = 2f;
    public float maxLaserDistance = 20f;

    [Header("References")]
    public LineRenderer laserRenderer;
    public Transform firePoint;
    public LayerMask groundLayer;
    public GameObject hitEffectPrefab;

    [Header("Damage Settings")]
    public float damage = 10f;

    private float timer;
    private bool isLaserActive = false;
    private GameObject currentHitEffect;
    private EdgeCollider2D edgeCollider;
    private AudioSource laserAudio;

    private void Start()
    {
        laserRenderer.enabled = false;
        edgeCollider = GetComponent<EdgeCollider2D>();
        edgeCollider.isTrigger = true;
        edgeCollider.enabled = false;

        laserAudio = GetComponent<AudioSource>();
        laserAudio.playOnAwake = false;

        timer = laserCooldownDuration;
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (!isLaserActive && timer <= 0f)
        {
            isLaserActive = true;
            timer = laserActiveDuration;
            laserRenderer.enabled = true;
            edgeCollider.enabled = true;

            // 🔊 Bật âm thanh khi laser được kích hoạt
            if (laserAudio != null) laserAudio.Play();
        }
        else if (isLaserActive && timer <= 0f)
        {
            isLaserActive = false;
            timer = laserCooldownDuration;
            laserRenderer.enabled = false;
            edgeCollider.enabled = false;
            HideHitEffect();
        }

        if (isLaserActive)
        {
            UpdateLaser();
        }
    }

    void UpdateLaser()
    {
        RaycastHit2D hit = Physics2D.Raycast(firePoint.position, firePoint.right, maxLaserDistance, groundLayer);

        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Ground"))
            {
                laserRenderer.enabled = true;
                edgeCollider.enabled = true;
                DrawLaser(firePoint.position, hit.point);
                ShowHitEffect(hit.point);
            }
            else if (hit.collider.CompareTag("Object"))
            {
                laserRenderer.enabled = false;
                edgeCollider.enabled = false;
                HideHitEffect();
            }
            else
            {
                laserRenderer.enabled = false;
                edgeCollider.enabled = false;
                HideHitEffect();
            }
        }
        else
        {
            laserRenderer.enabled = false;
            edgeCollider.enabled = false;
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
