using UnityEngine;

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

        Debug.Log("Shield activated!");
    }

    protected override void OnAbilityEnd()
    {
        if (shieldVisual != null)
            shieldVisual.SetActive(false);

        Debug.Log("Shield deactivated!");
    }
}
