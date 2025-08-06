using UnityEngine;

public class LyraDamageReceiver : DamageReceiver
{
    [SerializeField] private MonoBehaviour explodableTarget;
    private IExplodable explodable;

    protected override void ResetValue()
    {
        base.ResetValue();
        this.maxHP = 5;
    }

    protected override void LoadComponents()
    {
        base.LoadComponents();

        if (explodableTarget == null)
        {
            explodable = GetComponent<IExplodable>();
            explodableTarget = explodable as MonoBehaviour; // ✅ ép kiểu lại
        }
        else
        {
            explodable = explodableTarget as IExplodable;
        }

        if (explodable == null)
        {
            Debug.LogError($"{name} is missing a component that implements IExplodable.");
        }
    }

    protected override void Awake()
    {
        if (!TryGetComponent<IExplodable>(out explodable))
            Debug.LogError("This GameObject does not implement IExplodable: ." + gameObject.name);
    }


    protected override void OnDead()
    {
        base.OnDead();
        explodable?.Explode();
    }
}
