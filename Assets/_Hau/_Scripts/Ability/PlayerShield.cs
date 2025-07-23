/*using UnityEngine;

public class PlayerShield : PlayerAbility
{
    [Header("Shield Settings")]
    [SerializeField] private GameObject shieldVisual; 

    protected override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.Q))
        {
            TryUseAbility();
        }
    }

    protected override void OnAbilityStart()
    {
        if (shieldVisual != null)
            shieldVisual.SetActive(true);

        //Debug.Log("Shield activated!");
    }

    protected override void OnAbilityEnd()
    {
        if (shieldVisual != null)
            shieldVisual.SetActive(false);

        //Debug.Log("Shield deactivated!");
    }
}
*/

using UnityEngine;

public class PlayerShield : PlayerAbility
{
    [Header("Shield Settings")]
    [SerializeField] private GameObject shieldVisual;
    [SerializeField] private string shieldTag = "Shield"; // Tag để laser nhận diện

    protected override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.Q))
        {
            TryUseAbility();
        }
    }

    protected override void OnAbilityStart()
    {
        if (shieldVisual != null)
        {
            shieldVisual.SetActive(true);
            // Đảm bảo shield có Collider2D và đúng Layer/Tag
            SetupShieldCollider();
        }
    }

    protected override void OnAbilityEnd()
    {
        if (shieldVisual != null)
            shieldVisual.SetActive(false);
    }

    private void SetupShieldCollider()
    {
        if (shieldVisual != null)
        {
            // Thêm CircleCollider2D nếu chưa có
            CircleCollider2D shieldCollider = shieldVisual.GetComponent<CircleCollider2D>();
            if (shieldCollider == null)
            {
                shieldCollider = shieldVisual.AddComponent<CircleCollider2D>();
            }

            // Không phải trigger vì cần chặn laser
            shieldCollider.isTrigger = false;

            // Gắn tag để laser nhận diện
            shieldVisual.tag = shieldTag;
        }
    }
}