using UnityEngine;

public class Fire2Fly : EffectFlyToTarget
{
    protected override void ResetValue()
    {
        base.ResetValue();
        this.speed = 22f;
    }
}
