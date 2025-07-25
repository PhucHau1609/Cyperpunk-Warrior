using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class WindZone2D : MonoBehaviour
{
    public enum WindMode
    {
        AddForce,
        OverrideVelocity,
        GravityModifier
    }

    [Header("Wind Zone Settings")]
    public WindMode windMode = WindMode.AddForce;
    public Vector2 windDirection = Vector2.up;
    public float windStrength = 10f;
    public bool applyContinuously = true;

    [Header("Gravity Modifier Mode")]
    public float newGravityScale = 0.2f;
    private float originalGravityScale = 5f;

    [Header("Target Filter")]
    public string targetTag = "Player";
    //public string targetTag2 = "NPC";

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!applyContinuously && other.CompareTag(targetTag) /*&& other.CompareTag(targetTag2)*/)
        {
            ApplyWindEffect(other);
        }

        if (windMode == WindMode.GravityModifier && other.CompareTag(targetTag) /*&& other.CompareTag(targetTag2)*/)
        {
            var rb = other.attachedRigidbody;
            if (rb != null)
            {
                rb.gravityScale = newGravityScale;
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (applyContinuously && other.CompareTag(targetTag) /*&& other.CompareTag(targetTag2)*/)
        {
            ApplyWindEffect(other);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Khôi phục gravity mặc định nếu cần
        if (windMode == WindMode.GravityModifier && other.CompareTag(targetTag) /*&& other.CompareTag(targetTag2)*/)
        {
            var rb = other.attachedRigidbody;
            if (rb != null)
            {
                rb.gravityScale = originalGravityScale; // reset về mặc định (hoặc bạn có thể lưu lại giá trị ban đầu để khôi phục)
            }
        }
    }

    private void ApplyWindEffect(Collider2D other)
    {
        Rigidbody2D rb = other.attachedRigidbody;
        if (rb == null) return;

        switch (windMode)
        {
            case WindMode.AddForce:
                rb.AddForce(windDirection.normalized * windStrength, ForceMode2D.Force);
                break;
            case WindMode.OverrideVelocity:
                rb.linearVelocity = windDirection.normalized * windStrength;
                break;
            case WindMode.GravityModifier:
                // handled in Enter & Exit
                break;
        }
    }
}
