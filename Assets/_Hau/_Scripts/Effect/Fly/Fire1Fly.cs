using UnityEngine;

public class Fire1Fly : EffectFlyToTarget
{
    protected override void ResetValue()
    {
        base.ResetValue();
        this.speed = 22f;
    }
}
