using UnityEngine;

public class Fire4Fly : EffectFlyToTarget
{
    protected override void ResetValue()
    {
        base.ResetValue();
        this.speed = 22f;
    }
}
