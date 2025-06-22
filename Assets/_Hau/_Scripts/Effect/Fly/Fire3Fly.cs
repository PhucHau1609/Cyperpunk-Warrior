using UnityEngine;

public class Fire3Fly : EffectFlyToTarget
{
    protected override void ResetValue()
    {
        base.ResetValue();
        this.speed = 30f;
    }
}
